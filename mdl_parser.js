var fs = require('fs')

if (process.argv[2]) {
  get_model(fs.readFileSync(process.argv[2], "utf8"));
} else {
  console.log('');
  console.log('--------------------------');
  console.log('Error: Need mdl file path!');
  console.log('--------------------------');
  console.log('');
  console.log('-----------------------------------');
  console.log('Example:');
  console.log('  node mdl_parser.js verysimple.mdl');
  console.log('-----------------------------------');
  console.log('');
}

function get_model_name(content) {
  var reg = /Model\s+\{\s+Name\s+\"(\w+)\"/;
	return reg.exec(content);
}

function get_model(content) {
	
	var sp = '\r\n';

	if (content.indexOf('\r') < 0) {
		sp = '\n';
	}
  
  var lines = content.split(sp);
  i = 0;

  var context_switch = {
    'model': {
    	'reg': /\s*Model\s+\{/,
    	'handel': toModel
    },
    'block_default': {
    	'reg': /\s*BlockParameterDefaults\s+\{/,
    	'handel': toBlockDefault
    },
    'system': {
    	'reg': /\s*System\s+\{/,
    	'handel': toSystem
    },
    'block': {
    	'reg': /\s*Block\s+\{/,
    	'handel': toBlock
    },
    'line': {
    	'reg': /\s*Line\s+\{/,
    	'handel': toLine
    }
  }

  var keys = Object.keys(context_switch);

  var current = new MDL('no name');
  var q = current;
  var index = 0;

  while ( i < lines.length ) {
    var line = lines[i++];
    var flag = true;
    for (var j = keys.length - 1; j >= 0; j--) {
      var reg = context_switch[keys[j]].reg;
      var result = reg.exec(line);
      if ( result ) {
      	var r = context_switch[keys[j]].handel(current, index);
        current = r[0];
        index = r[1];
        current.content = {};
        flag = false;
        break;
      }
    }
    
    if ( flag && line.indexOf('}') < 0 ) {
      var parameter_reg = /\s+(\w+)\s+\"?([\[\d\s\w\,\;\-\]]+)\"?/;
      var m = parameter_reg.exec(line);
      if ( m ) {
      	current.content[m[1]] = m[2];
			}
    }
  }

  //console.log(q.System.lines[1].content);
  var sdf = new SDF(q);
  console.log(JSON.stringify(sdf));
  process.exit();
  //console.log(sdf); 
}

function SDF(parsed_mdl) {
	this.type = 'SDF';
	this.nodes = {};
	this.edges = {};

	var blocks = parsed_mdl.BlockParameterDefaults.blocks;
	var default_blocks = {};
	for (var i = blocks.length - 1; i >= 0; i--) {
	  default_blocks[blocks[i].content.BlockType] = blocks[i].content;
	};

	var blocks = parsed_mdl.System.blocks;
	var system_blocks = {};
	var block_parameters = ['BlockType', 'SID', 'Port', 'Ports', 'Name', 'Value'];

	for (var i = blocks.length - 1; i >= 0; i--) {
	  system_blocks[blocks[i].content.Name] = default_blocks[blocks[i].content.BlockType];
	  
	  Object.keys(blocks[i].content).forEach( function (parameter_name) {
      system_blocks[blocks[i].content.Name][parameter_name] = blocks[i].content[parameter_name];
    });
    var b = {};
    block_parameters.forEach( function(p) { b[p] = system_blocks[blocks[i].content.Name][p]; });
    system_blocks[blocks[i].content.Name] = b;
	};
  this.nodes = system_blocks;

  var lines = parsed_mdl.System.lines;
  var system_lines = {};
	for (var i = lines.length - 1; i >= 0; i--) {
	  system_lines[i] = lines[i].content;
	  delete system_lines[i].Points;
	};
  this.edges = system_lines;
  
  //console.log(system_blocks);	
}

function toModel(current, index) {
	return [current, index];
}

function toBlockDefault(current, index) {
	var b = {};
	b.type = 'BlockParameterDefaults';
	b.parent = current;
	b.blocks = [];

	current.BlockParameterDefaults = b;
	return [b, 0];
}

function toSystem(current, index) {
	var b = {};
	b.type = 'System';
	b.parent = current.parent.parent;
	b.blocks = [];
	b.lines = [];

	current.parent.parent.System = b;
	return [b, 0];
}

function toBlock(current, index) {
	var b = {};
  b.type = 'Block';

	if ( current.type == 'BlockParameterDefaults' | current.type == 'System' ) {
	  b.parent = current;
	  current.blocks.push(b);
	} else {
		b.parent = current.parent;
		current.parent.blocks.push(b);
	}

	return [b, index+1];
}

function toLine(current, index) {
	var l = {};
  l.type = 'Line';

	if ( current.type == 'System' ) {
	  l.parent = current;
	  current.lines.push(l);
	} else {
		l.parent = current.parent;
		current.parent.lines.push(l);
	}

	return [l, index+1];
}

function MDL() {
	this.type = 'Model';
}

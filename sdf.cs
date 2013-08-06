using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web.Script.Serialization;

namespace SDF {

  // Data dependency class
  public class Dependency
  {
    public int ParentID;
   	public List<int> SourcePorts;
  	public List<int> DestinationPorts;
        
    //+ Constructors
	  	public Dependency( int parent_id, List<int> source_ports, List<int> destination_ports )
	  	{
	  		ParentID = parent_id;
	  		if (source_ports.Count == 1 || destination_ports.Count == 1) {
	  		  SourcePorts = source_ports;
	  		  DestinationPorts = destination_ports;
	  	  } else {
	  	    MultipleToMultipleDataDependency(source_ports, destination_ports);
	  	  }
	  	}

	  	public Dependency( int parent_id, int source_port, int destination_port )
	  	{
	  		ParentID = parent_id;
	  		SourcePorts = new List<int>(){ source_port };
	  		DestinationPorts = new List<int>(){ destination_port };
	    }

	    // N -> M data dependency is not implemented yet
	  	// public MapPattern SourceToDestination;
	  	private void MultipleToMultipleDataDependency(
	  		List<int> source_ports,
	  		List<int> destination_ports 
		  	)
	  	{
		  		throw new System.NotImplementedException(
		  	    	"N->M data dependecy is not implemented yet" );
		  }

		public override string ToString()
		{
			string result = "";
			result += "######## Dependency ########";
			
			result += "\nParent ID: " + ParentID + "\n";
			
			result += "  Source Ports: \n    {";
			
			SourcePorts.ForEach( p => result += "" + p + ", " );
			if (SourcePorts.Count > 0)
			  result = result.Substring(0, result.Length-2);
			result += "}\n";
			
			result += "  Destination Ports: \n    {";
			DestinationPorts.ForEach( p => result += "" + p + ", " );
			if (DestinationPorts.Count > 0)
			  result = result.Substring(0, result.Length-2);
			result += "}\n";

			result += "############################";

			return result;			
		}
  }

  // SDF edge class
	public class SDFEdge
	{

    public int SourceID;
    public int SourcePort;
    public int SourceToken;
    public int DestinationID;
    public int DestinationPort;
    public int DestinationToken;
    
    //+ Constructors
	    public SDFEdge(
	    	int src_id, int src_port, int src_token,
	    	int dest_id, int dest_port, int dest_token)
	    {
	    	SourceID = src_id;
	    	SourcePort = src_port;
	    	SourceToken = src_token;
	    	DestinationID = dest_id;
	    	DestinationPort = dest_port;
	    	DestinationToken = dest_token;
	    }

	    public SDFEdge()
	    {
	    	SourceID = -1;
	    	SourcePort = -1;
	    	SourceToken = -1;
	    	DestinationID = -1;
	    	DestinationPort = -1;
	    	DestinationToken = -1;
	    }
    
    public override string ToString()
    {
    	string result = "";
    	result += "######## SDFEdge ########\n";
      result += "Node " + SourceID;
      result += " Port " + SourcePort;
      result += " Token " + SourceToken + " ->\n  ";
      result += "Node " + DestinationID;
      result += " Port " + DestinationPort;
      result += " Token " + DestinationToken;
			result += "\n#########################";

    	return result;
    }
	}

  // SDF node base class
	public class SDFNode
	{
    //+ Node information 
	    public string Name;
			public int ID;

			public string RefName;
			public int RefID;

			public List<int> InPorts;
  		public List<int> OutPorts;
		
		//+ Data dependency information
			public List<Dependency> DependencyList;
			public List<int> ChildrenIDList;

			public void AddDependency( Dependency d ) { DependencyList.Add(d); }
			public void AddDependency( SDFNode parent, int source, int sink ) {
			  DependencyList.Add( new Dependency( parent.ID, source, sink ) );
			}

			public void AddChild( int c ) { ChildrenIDList.Add(c); }
		
		//+ Constructor
			public SDFNode( string name ) {
				construct(
					name, name,
			    -1, -1,
			    new List<Dependency>(), new List<int>(),
			    new List<int>(), new List<int>() );
			}
	    
	    public SDFNode( string name, string ref_name, int id = -1, int ref_id = -1) {
				construct(
					name, ref_name,
			    id, ref_id,
			    new List<Dependency>(), new List<int>(),
			    new List<int>(), new List<int>() );
			}

			public SDFNode( string name, int id ) {
			 	construct(
					name, name,
			    id, id,
			    new List<Dependency>(), new List<int>(),
			    new List<int>(), new List<int>() );
			}

			public SDFNode() {
				construct(
					String.Empty, String.Empty,
			    -1, -1,
			    new List<Dependency>(), new List<int>(),
			    new List<int>(), new List<int>() );
			}

			public SDFNode( 
				string name, string ref_name,
				int id, int ref_id,
				List<Dependency> dependencies, List<int> children,
				List<int> inports, List<int> outports ) {
			
				construct(
					name, ref_name,
			    id, ref_id,
			    new List<Dependency>(), new List<int>(),
			    new List<int>(), new List<int>() );
			}

			private void construct(
				string name, string ref_name,
				int id, int ref_id,
				List<Dependency> dependencies, List<int> children,
				List<int> inports, List<int> outports) {

				Name = name;
				ID = id;
				RefName = ref_name;
				RefID = ref_id;
				InPorts = inports;
				OutPorts = outports;
				DependencyList = dependencies;
				ChildrenIDList = children;
			}

	  public override string ToString()
	  {
      string result =  "######## SDFNode ########\n";

      result +=        "Name: " + Name + "\n";
      result +=        "ID: " + ID + "\n";
      result +=        "RefName: " + RefName + "\n";
      result +=        "RefID: " + RefID + "\n";
      
			result +=        "  Input Ports: \n    {";
			InPorts.ForEach( p => result += "" + p + ", " );
			if (InPorts.Count > 0)
			  result = result.Substring(0, result.Length-2);
			result += "}\n";
			
			result +=        "  Output Ports: \n    {";
			OutPorts.ForEach( p => result += "" + p + ", " );
			if (OutPorts.Count > 0)
			  result = result.Substring(0, result.Length-2);
			result +=        "}\n";
			
			result +=        "#########################";

			return result;
	  }
	}
  
  // Unbalanced raw SDF graph class
	public class SDFGraph
	{
    
    //+ Node and edge sets
	    public List<SDFNode> NodeSet;
	    public List<SDFEdge> EdgeSet;
    
    //+ Constructors
	    public SDFGraph( List<SDFNode> nodes, List<SDFEdge> edges ) {
	    	NodeSet = new List<SDFNode>();
	    	NodeSet.AddRange(nodes);
	    	EdgeSet = new List<SDFEdge>();
	    	EdgeSet.AddRange(edges);
	    }

			public SDFGraph() {
	    	NodeSet = new List<SDFNode>();
	    	EdgeSet = new List<SDFEdge>();
			}

	    public static SDFGraph CreateByMDL( string mdl_filename, string nodepath = "node.exe")
	    {
				Process p = new Process();
				p.StartInfo.FileName = nodepath;
				p.StartInfo.Arguments = mdl_filename;
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.RedirectStandardInput = true;
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.CreateNoWindow = true;
				p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				p.Start();

	      string t = p.StandardOutput.ReadToEnd();
				
	      JavaScriptSerializer serializer = new JavaScriptSerializer();
				Dictionary<string, object> result = (Dictionary<string, Object>)(serializer.DeserializeObject(t));

				var g = new SDFGraph();
        
        // Nodes
	      Dictionary<string, object> nodes = (Dictionary<string, object>)result["nodes"];
	      foreach (string block_name in nodes.Keys)
	      {
	      	Dictionary<string, object> one_block = (Dictionary<string, object>)nodes[block_name];
	      	var sdf_node = new SDFNode();

	      	foreach (string parameter in one_block.Keys)
	      	{
	      		string ps = (string)one_block[parameter];
						
						switch (parameter)
						{
							case "BlockType":
							  sdf_node.RefName = ps;
							  break;
							case "Name":
							  sdf_node.Name = ps;
							  break;
							case "SID":
							  sdf_node.ID = int.Parse(ps);
							  break;
							case "Port":
							  for(int i = 0; i < int.Parse(ps); i++)
							    sdf_node.OutPorts.Add(1);
							  break;
							case "Ports":
							  var io = ps.Substring(1, ps.Length - 2).Split(',');
							  var inputs = int.Parse(io[0]);
							  var outputs = int.Parse(io[1]);
								for(int i = 0; i < inputs; i++)
							    sdf_node.InPorts.Add(1);
							  for(int i = 0; i < outputs; i++)
							    sdf_node.OutPorts.Add(1);
							  break;
							default:
							  break;
						}
	      	}

	      	g.NodeSet.Add(sdf_node);
	      }

        // Edges
	      Dictionary<string, object> edges = (Dictionary<string, object>)result["edges"];
	      foreach (string edge_index in edges.Keys)
	      {
	      	Dictionary<string, object> one_edge = (Dictionary<string, object>)edges[edge_index];
	      	var sdf_edge = new SDFEdge(){SourceToken = 1, DestinationToken = 1};

	      	foreach (string parameter in one_edge.Keys)
	      	{
	      		string ps = (string)one_edge[parameter];
						
						switch (parameter)
						{
							case "SrcBlock":
							  sdf_edge.SourceID = g.NodeSet.Find(n => n.Name == ps).ID;
							  break;
							case "SrcPort":
							  sdf_edge.SourcePort = int.Parse(ps);
							  break;
							case "DstBlock":
							  sdf_edge.DestinationID = g.NodeSet.Find(n => n.Name == ps).ID;;
							  break;
							case "DstPort":
							  sdf_edge.DestinationPort = int.Parse(ps);
							  break;
							default:
							  break;
						}
	      	}

	      	g.EdgeSet.Add(sdf_edge);
	      }

	      p.WaitForExit();
	      p.Close();
	      return g;      
	    }

    //+ Methods to add node and edge
	    public void AddNode(SDFNode node) {
	    	NodeSet.Add(node);
	    	node.DependencyList.ForEach( d =>
	    	{
	    		if ( NodeSet.Exists( n => n.ID == d.ParentID ) )
	    		  AddEdge(node, d);
	    	});    	
	    }
	    
	    private void AddEdge( SDFNode destination, Dependency dependency ) {
	      if (dependency.SourcePorts.Count == 1 && dependency.DestinationPorts.Count  == 1)
	        AddEdgeForSingleDataDependency(destination, dependency);
	      else
	        throw new System.NotImplementedException(
		  	  	"N->1, 1->N and N->M data dependecies are not implemented yet" );
	    }

	    private void AddEdgeForSingleDataDependency( SDFNode destination, Dependency dependency ) {
				EdgeSet.Add( new SDFEdge() {
	    		SourceID = dependency.ParentID,
	    		SourcePort = dependency.SourcePorts[0],
	    		SourceToken = NodeSet[dependency.ParentID].OutPorts[dependency.SourcePorts[0]],
	    		DestinationID = destination.ID,
	    		DestinationPort = dependency.DestinationPorts[0],
	    		DestinationToken = destination.InPorts[dependency.DestinationPorts[0]]
	    		});
			}
    
    public override string ToString()
    {
    	string result = "";
    	result +=   "######## SDFGraph ########\n";
			NodeSet.ForEach(n => result += "" + n + "\n");
  		EdgeSet.ForEach(e => result += "" + e + "\n");
  		result += "\n##########################";
  		return result;
    }	
	}

}

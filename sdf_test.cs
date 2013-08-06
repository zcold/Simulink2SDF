using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web.Script.Serialization;
using SDF;

namespace test {
  // test
	public class program {
		public static void Main(string[] args) {
			var mdl_file = "verysimple.mdl";
      if (args.Length > 0)
  		  mdl_file = args[0];
      var g = MDL_Parser.CreateSDFGraph(mdl_file);
      Console.WriteLine(g);
    }
	}
}

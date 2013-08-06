using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web.Script.Serialization;

namespace SDF {

  public static class MDL_Parser
  {
    public static SDFGraph CreateSDFGraph( string mdl_filename, string nodepath = "node.exe")
    {
  		Process p = new Process();
			p.StartInfo.FileName = nodepath;
			p.StartInfo.Arguments = "mdl_parser.js " + mdl_filename;
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
	}

}

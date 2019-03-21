using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RSG;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace UIWidgetsGallery.gallery {

    public class ExampleCodeParser {
        const string _kStartTag = "// START ";
        const string _kEndTag = "// END";
    
        Dictionary<string, string> _exampleCode;
    
        public string getExampleCode(string tag, AssetBundle bundle) {
            if (this._exampleCode == null) {
                this._parseExampleCode(bundle);
            }
            return this._exampleCode.getOrDefault(tag);
        }
    
        void _parseExampleCode(AssetBundle bundle) {
            string code = Resources.Load<TextAsset>("example_code.cs")?.text ??
                                "// example_code.cs not found\n";
            this._exampleCode = new Dictionary<string, string>{};
    
            List<String> lines = code.Split('\n').ToList();
    
            List<String> codeBlock = null;
            string codeTag = null;
    
            foreach (string line in lines) {
                if (codeBlock == null) {
                    if (line.StartsWith(_kStartTag)) {
                        codeBlock = new List<String>();
                        codeTag = line.Substring(_kStartTag.Length).Trim();
                    } else {
                    }
                } else {
                    if (line.StartsWith(_kEndTag)) {
                        this._exampleCode[codeTag] = string.Join("\n", codeBlock);
                        codeBlock = null;
                        codeTag = null;
                    } else {
                        codeBlock.Add(line.TrimEnd());
                    }
                }
            }
        }
    }

}
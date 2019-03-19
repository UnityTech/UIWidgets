using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RSG;
using UnityEngine;

namespace UIWidgetsGallery.gallery {

    public class ExampleCodeParser {
        const string _kStartTag = "// START ";
        const string _kEndTag = "// END";
    
        Dictionary<string, string> _exampleCode;
    
        public IPromise<string> getExampleCode(string tag, AssetBundle bundle) {
            if (this._exampleCode == null)
                this._parseExampleCode(bundle);
            return Promise<string>.Resolved(this._exampleCode[tag]);
        }
    
        async Task _parseExampleCode(AssetBundle bundle) {
            string code = Resources.Load<TextAsset>("example_code.cs")?.text ??
                                "// example_code.cs not found\n";
            this._exampleCode = new Dictionary<string, string>{};
    
            List<String> lines = code.Split('\n').ToList();
    
            List<String> codeBlock = null;
            string codeTag = null;
    
            foreach (string line in lines) {
                if (codeBlock == null) {
                    // Outside a block.
                    if (line.StartsWith(_kStartTag)) {
                        // Starting a new code block.
                        codeBlock = new List<String>();
                        codeTag = line.Substring(_kStartTag.Length).Trim();
                    } else {
                        // Just skipping the line.
                    }
                } else {
                    // Inside a block.
                    if (line.StartsWith(_kEndTag)) {
                        // Add the block.
                        this._exampleCode[codeTag] = string.Join("\n", codeBlock);
                        codeBlock = null;
                        codeTag = null;
                    } else {
                        // Add to the current block
                        // trimRight() to remove any \r on Windows
                        // without removing any useful indentation
                        codeBlock.Add(line.TrimEnd());
                    }
                }
            }
        }
    }

}
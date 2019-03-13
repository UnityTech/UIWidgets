using System;
using System.Collections.Generic;
using RSG;
using UnityEngine;

namespace UIWidgetsGallery.gallery {

    public class ExampleCodeParser {
        const string _kStartTag = "// START ";
        const string _kEndTag = "// END";
    
        Dictionary<string, string> _exampleCode;
    
        async IPromise<string> getExampleCode(string tag, AssetBundle bundle) {
            if (_exampleCode == null)
                await _parseExampleCode(bundle);
            return _exampleCode[tag];
        }
    
        Future<void> _parseExampleCode(AssetBundle bundle) async {
            final String code = await bundle.loadString("lib/gallery/example_code.dart") ??
                                "// lib/gallery/example_code.dart not found\n";
            _exampleCode = <String, String>{};
    
            final List<String> lines = code.split("\n");
    
            List<String> codeBlock;
            String codeTag;
    
            for (String line in lines) {
                if (codeBlock == null) {
                    // Outside a block.
                    if (line.startsWith(_kStartTag)) {
                        // Starting a new code block.
                        codeBlock = <String>[];
                        codeTag = line.substring(_kStartTag.length).trim();
                    } else {
                        // Just skipping the line.
                    }
                } else {
                    // Inside a block.
                    if (line.startsWith(_kEndTag)) {
                        // Add the block.
                        _exampleCode[codeTag] = codeBlock.join("\n");
                        codeBlock = null;
                        codeTag = null;
                    } else {
                        // Add to the current block
                        // trimRight() to remove any \r on Windows
                        // without removing any useful indentation
                        codeBlock.add(line.trimRight());
                    }
                }
            }
        }
    }

}
var path = require('path');
var chalk = require('chalk');
var glob = require("glob");
var fs = require('fs');
var nunjucks = require('nunjucks');
var chokidar = require('chokidar');

exports.command = 'codegen [dir]';
exports.desc = "generate mixin code";
exports.builder = function (yargs) {
    return yargs.positional('dir', {
        describe: 'the working directory',
        type: 'string',
        default: '.'
    }).option('watch', {
        alias: 'w',
        describe: 'Watch for file changes',
        boolean: true
    });
};

exports.handler = function (argv) {
    var cwd = path.resolve(__dirname, '../..', argv.dir);
    var data = {};
    var env = nunjucks.configure(cwd, {
        trimBlocks: true,
        lstripBlocks: true,
        noCache: true
    });
    
    glob('**/*.njk', {
        strict: true,
        cwd: cwd,
        ignore: '**/_*.*'
    }, function (err, files) {
        if (err) {
            return console.error(chalk.red(err));
        }

        renderAll(env, cwd, files, data);
    });
    
    if (argv.watch) {
        var watcher = chokidar.watch('**/*.njk', {
            persistent: true,
            cwd: cwd,
            awaitWriteFinish: {
                stabilityThreshold: 300,
                pollInterval: 50
            }
        });

        var layouts = [];
        var templates = [];

        watcher.on('ready', function () {
            console.log(chalk.gray('Watching templates...'));
        });

        watcher.on('add', function (file) {
            if (path.basename(file).indexOf('_') === 0) {
                layouts.push(file);
            } else {
                templates.push(file);
            }
        });

        watcher.on('change', function (file) {
            if (layouts.indexOf(file) >= 0) {
                renderAll(env, cwd, templates, data);
            } else {
                render(env, cwd, file, data);
            }
        });
    }
};

function render(env, cwd, file, data) {
    env.render(file, data, function (err, res) {
        if (err) {
            return console.error(chalk.red(err));
        }

        var outputFile = file.replace(/\.\w+$/, '') + '.gen.cs';
        console.log(chalk.blue('Rendering: ' + file));
        fs.writeFileSync(path.resolve(cwd, outputFile), res);
    });
}

function renderAll(env, cwd, files, data) {
    for (var i = 0; i < files.length; i++) {
        render(env, cwd, files[i], data);
    }
}
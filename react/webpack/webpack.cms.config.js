const path = require("path");

module.exports = (env, argv) => {
    const webpackCms = require("./webpack.common")(env, argv);
    const amdModules = ["require", "dojo/", "dijit/", "epi/", "epi-cms/"];

    webpackCms.entry = {
        formImagePreviewWidget: "./src/cms/widgets/formImagePreviewWidget.tsx",
        environmentHighlighterWidget: "./src/cms/widgets/environmentHighlighterWidget.tsx",
    };

    webpackCms.output = {
        libraryTarget: "amd",
        libraryExport: "default",
        filename: `[name].js`,
        path: path.join(__dirname, `../../src/lunchin.Optimizely.Cloud.Extensions/clientResources/widgets`),
        publicPath: "",
    };

    webpackCms.externals = [
        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        function ({ context, request }, callback) {
            if (request && amdModules.some((x) => request.startsWith(x))) {
                // Externalize to a commonjs module using the request path
                return callback(null, request);
            }

            // Continue without externalizing the import
            callback();
        },
    ];

    return webpackCms;
};

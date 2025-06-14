const path = require("path");
module.exports = (env, argv) => {
    const webpackStorageExplorerConfig = require("./webpack.common")(env, argv);

    webpackStorageExplorerConfig.entry = {
        storageExplorer: "./src/apps/storageExplorer/main.tsx",
    };

    webpackStorageExplorerConfig.output = {
        filename: `[name].bundle.js`,
        path: path.join(__dirname, `../../src/lunchin.Optimizely.Cloud.Extensions/clientResources/apps`),
    };

    return webpackStorageExplorerConfig;
};

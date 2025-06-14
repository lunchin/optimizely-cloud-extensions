const path = require("path");
module.exports = (env, argv) => {
    const webpackMasterLanguageConfig = require("./webpack.common")(env, argv);

    webpackMasterLanguageConfig.entry = {
        masterlanguage: "./src/apps/masterlanguage/main.tsx",
    };

    webpackMasterLanguageConfig.output = {
        filename: `[name].bundle.js`,
        path: path.join(__dirname, `../../src/lunchin.Optimizely.Cloud.Extensions/clientResources/apps`),
    };

    return webpackMasterLanguageConfig;
};

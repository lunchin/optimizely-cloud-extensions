const path = require("path");
module.exports = (env, argv) => {
    const webpackMultiCouponConfig = require("./webpack.common")(env, argv);

    webpackMultiCouponConfig.entry = {
        multicoupon: "./src/apps/multicoupon/main.tsx",
    };

    webpackMultiCouponConfig.output = {
        filename: `[name].bundle.js`,
        path: path.join(__dirname, `../../src/lunchin.Optimizely.Cloud.Extensions.Commerce/clientResources/apps`),
    };

    return webpackMultiCouponConfig;
};

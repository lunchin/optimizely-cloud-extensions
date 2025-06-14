const path = require("path");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const extensions = [".ts", ".tsx", ".js", ".jsx", ".scss", ".css", ".json"];
const getWebpackConfig = (env, argv) => {
    const buildSourceMaps = !!env.outputPostfix;
    const isProduction = argv.mode === "production";

    return {
        mode: argv.mode,
        module: {
            rules: [
                {
                    test: /\.tsx?$/,
                    exclude: /node_modules/,
                    use: [
                        {
                            loader: "ts-loader",
                            options: {
                                transpileOnly: true,
                            },
                        },
                    ],
                },
                {
                    test: /\.(png|svg|jpg|jpeg|gif)$/i,
                    type: "asset/resource",
                },
                {
                    test: /\.svg$/,
                    type: "asset/inline",
                },
                {
                    test: /\.s?css/,
                    exclude: '/node_modules/@mantine/core/',
                    use: [MiniCssExtractPlugin.loader, "css-loader", "postcss-loader", "sass-loader"],
                },
            ],
        },
        resolve: {
            extensions: extensions,
            modules: [path.resolve(__dirname, "src"), "node_modules"],
            fallback: {
                fs: false,
                tls: false,
                net: false,
                path: false,
                zlib: false,
                http: false,
                https: false,
            },
        },
        cache: {
            type: "filesystem", // use file cache
        },
        devtool: isProduction ? (buildSourceMaps ? "source-map" : false) : "inline-source-map",
        plugins: [new MiniCssExtractPlugin()],
    };
};

module.exports = (env, argv) => {
    const webpackCommonConfig = getWebpackConfig(env, argv);
    return webpackCommonConfig;
};

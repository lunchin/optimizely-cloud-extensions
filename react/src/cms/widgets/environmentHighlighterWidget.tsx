import { createRoot, Root } from "react-dom/client";
import declare from "dojo/_base/declare";
import WidgetBase from "dijit/_WidgetBase";
import EnvironmentHighlighter from "../components/environmentHighlighter";

export default declare([WidgetBase], {
    root: null,
    postCreate() {
        this.root = createRoot(this.domNode);
        (this.root as Root).render(
            <EnvironmentHighlighter
                environment={this.environment}
                backgroundColor={this.backgroundColor}
                textColor={this.textColor}
            />
        );
    },

    destroy() {
        (this.root as Root).unmount();
    },
});

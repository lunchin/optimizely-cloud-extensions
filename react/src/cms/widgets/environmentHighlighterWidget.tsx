import ReactDOM from "react-dom";
import { createRoot } from "react-dom/client";
import declare from "dojo/_base/declare";
import WidgetBase from "dijit/_WidgetBase";
import EnvironmentHighlighter from "../components/environmentHighlighter";

export default declare([WidgetBase], {
    postCreate() {
        const root = createRoot(this.domNode);
        root.render(
            <EnvironmentHighlighter
                environment={this.environment}
                backgroundColor={this.backgroundColor}
                textColor={this.textColor}
            />
        );
    },

    destroy() {
        ReactDOM.unmountComponentAtNode(this.domNode);
    },
});

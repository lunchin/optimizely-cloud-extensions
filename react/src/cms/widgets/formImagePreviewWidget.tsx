import { createRoot } from "react-dom/client";
import ReactDOM from "react-dom";
import declare from "dojo/_base/declare";
import WidgetBase from "dijit/_WidgetBase";
import FormImagePreview from "../components/formImagePreview";

export default declare([WidgetBase], {
    // editor descriptor variables
    src: null,

    postCreate() {
        const root = createRoot(this.domNode);
        root.render(<FormImagePreview text={this.src} />);
    },

    destroy() {
        ReactDOM.unmountComponentAtNode(this.domNode);
    },
});

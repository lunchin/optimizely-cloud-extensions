import { createRoot, Root } from "react-dom/client";
import declare from "dojo/_base/declare";
import WidgetBase from "dijit/_WidgetBase";
import FormImagePreview from "../components/formImagePreview";

export default declare([WidgetBase], {
    // editor descriptor variables
    src: null,
    root: null,
    postCreate() {
        this.root = createRoot(this.domNode);
        (this.root as Root).render(<FormImagePreview text={this.src} />);
    },

    destroy() {
        (this.root as Root).unmount();
    },
});

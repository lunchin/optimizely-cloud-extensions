import { createRoot } from "react-dom/client";
import ContextLanguageWrapper from "../../lang/contextLanguageWrapper";
import App from "./app";

const root = document.getElementById("root") as HTMLElement;
const rootElement = createRoot(root);
rootElement.render(
    <ContextLanguageWrapper>
        <App />
    </ContextLanguageWrapper>
);

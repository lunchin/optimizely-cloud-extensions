import React from "react";
import { IntlProvider } from "react-intl";
import English from "./en.json";

export type jsonData = typeof English;

const Context = React.createContext<object | null>(null);

const getLanguage = (): string => {
    let shortLang = navigator.language;
    if (shortLang.indexOf("-") !== -1) {
        shortLang = shortLang.split("-")[0];
    }
    if (shortLang.indexOf("_") !== -1) {
        shortLang = shortLang.split("_")[0];
    }
    return shortLang;
};

const userLocale = getLanguage();
let lang: jsonData;
if (userLocale === "en") {
    lang = English;
} /* else if (userLocale === "fr") {
    lang = French;
} */

const contextLanguageWrapper = (props: {
    children:
        | string
        | number
        | boolean
        | React.ReactElement<any, string | React.JSXElementConstructor<any>>
        | Iterable<React.ReactNode>
        | React.ReactPortal
        | null
        | undefined;
}) => {
    return (
        <Context.Provider value={{ userLocale }}>
            <IntlProvider messages={lang} locale={userLocale}>
                {props.children}
            </IntlProvider>
        </Context.Provider>
    );
};

export default contextLanguageWrapper;

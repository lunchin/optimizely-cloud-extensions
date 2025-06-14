import { createRoot } from "react-dom/client";
import ContextLanguageWrapper from "../../lang/contextLanguageWrapper";
import App from "./app";
import { createHashRouter, createRoutesFromElements, Route, RouterProvider } from "react-router-dom";
import Promotions from "./views/promotions";
import EditCoupons from "./views/editCoupons";
import React from "react";

const router = createHashRouter(
    createRoutesFromElements(
        <Route>
            <Route path="/" element={<App />}>
                <Route index element={<Promotions />} />
                <Route path="edit/:id" element={<EditCoupons />} />
            </Route>
        </Route>
    )
);

const root = document.getElementById("root") as HTMLElement;
const rootElement = createRoot(root);
rootElement.render(
    <React.StrictMode>
        <ContextLanguageWrapper>
            <RouterProvider router={router} />
        </ContextLanguageWrapper>
    </React.StrictMode>
);

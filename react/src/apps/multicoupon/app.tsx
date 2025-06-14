import { Outlet } from "react-router-dom";
import Error from "./error";
import { ErrorBoundary } from "react-error-boundary";
import "./styles/app.scss";

const App = () => {
    return (
        <>
            <ErrorBoundary FallbackComponent={Error}>
                <Outlet />
            </ErrorBoundary>
        </>
    );
};

export default App;

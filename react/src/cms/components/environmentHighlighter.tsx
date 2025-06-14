interface EnvironmentHighlighterProps {
    environment: string;
    backgroundColor: string;
    textColor: string;
}

const EnvironmentHighlighter = (props: EnvironmentHighlighterProps) => {
    const { environment, backgroundColor, textColor } = props;
    return (
        <div style={{ textAlign: "center" }}>
            <div style={{ color: textColor, backgroundColor: backgroundColor, margin: "0 auto", width: "400px" }}>
                You are working on {environment} environment
            </div>
        </div>
    );
};

export default EnvironmentHighlighter;

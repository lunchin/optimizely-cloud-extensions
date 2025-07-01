import React from "react";
interface FormImagePreviewProps {
    text: string;
}

const FormImagePreview = (props: FormImagePreviewProps) => {
    const { text } = props;
    return <img src={text} alt="error" />;
};

export default FormImagePreview;

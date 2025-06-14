import { FunctionComponent, useEffect, useState } from "react";
import DataTable, { TableColumn } from "react-data-table-component";
import { FormattedMessage } from "react-intl";
import "./app.scss";
import { ErrorMessage } from "apps/errorMessage";

export interface BlobItem {
    name: string;
    lastModified: string;
    size: number;
    contentType: string | null;
    blobType: string | null;
    status: string | null;
    url: string | null;
    isContainer: boolean;
    isDirectory: boolean;
    isBlob: boolean;
    etag: string | null;
    sizeString: string | null;
}

const App: FunctionComponent = () => {
    const [error, setError] = useState<ErrorMessage | null>(null);
    const [data, setData] = useState([]);
    const [isLoaded, setIsLoaded] = useState(false);
    const [container, setContainer] = useState<string | null>(null);
    const [createContainerName, setCreateContainerName] = useState<string | null>(null);
    const [path, setPath] = useState<string | null>(null);
    const [breadCrumbs, setBreadCrumbs] = useState<{ [key: string]: string } | null>(null);
    const [file, setFile] = useState<File | null>(null);
    const baseApi = window.location.protocol + "//" + window.location.host + "/api/storageexplorer/";
    const root = document.getElementById("root") as HTMLElement;
    const antiforgeryFormFieldName: string = root.dataset.epiAntiforgeryFormFieldName as string;
    const antiforgeryFormField: HTMLInputElement = document.getElementsByName(
        antiforgeryFormFieldName
    )[0] as HTMLInputElement;

    const columns: TableColumn<BlobItem>[] = [
        {
            name: <FormattedMessage id="name" />,
            cell: (row) => <a onClick={() => handleNameClick(row)}>{row.name}</a>,
            grow: 2,
        },
        {
            name: <FormattedMessage id="modified" />,
            selector: (row) => row.lastModified,
        },
        {
            name: <FormattedMessage id="size" />,
            selector: (row) => row.size,
        },
        {
            name: <FormattedMessage id="contentType" />,
            selector: (row) => row.contentType ?? "",
        },
        {
            name: <FormattedMessage id="blobType" />,
            selector: (row) => row.blobType ?? "",
        },
        {
            name: <FormattedMessage id="status" />,
            selector: (row) => row.status ?? "",
        },
        {
            name: <FormattedMessage id="etag" />,
            selector: (row) => row.etag ?? "",
        },
        {
            name: <FormattedMessage id="actions" />,
            cell: (row) => (
                <a onClick={() => handleDeleteClick(row)}>
                    <FormattedMessage id="delete" />
                </a>
            ),
        },
    ];

    useEffect(() => {
        search();
    }, [container, path]);

    const search = () => {
        let url = `${baseApi}search`;

        if (path) {
            url = url.includes("?") ? `${url}&path=${path}` : `${url}?path=${path}`;
        }

        if (container) {
            url = url.includes("?") ? `${url}&container=${container}` : `${url}?container=${container}`;
        }

        fetch(url)
            .then((res) => res.json())
            .then(
                (result) => {
                    setIsLoaded(true);
                    setData(result.results);
                    setBreadCrumbs(result.breadCrumbs);
                },
                (error) => {
                    setIsLoaded(true);
                    setError(error);
                }
            );
    };

    const handleNameClick = (row: BlobItem) => {
        if (row.isContainer) {
            setContainer(row.name);
        } else if (row.isDirectory) {
            setPath(row.name);
        } else {
            const encoded = encodeURI(row.url ?? "");
            let filename = "";
            fetch(`${baseApi}download?url=${encoded}`)
                .then((result) => {
                    if (!result.ok) {
                        throw Error(result.statusText);
                    }

                    const header = result.headers.get("Content-Disposition");
                    const parts = header?.split(";") ?? "";
                    filename = parts[1].split("=")[1].replaceAll('"', "");

                    return result.blob();
                })
                .then((blob) => {
                    if (blob != null) {
                        const url = window.URL.createObjectURL(blob);
                        const a = document.createElement("a");
                        a.href = url;
                        a.download = filename;
                        document.body.appendChild(a);
                        a.click();
                        a.remove();
                    }
                })
                .catch((err) => {
                    console.log(err);
                });
        }
    };

    const handleDeleteClick = (row: BlobItem) => {
        const encoded = encodeURI(row.url ?? row.name);
        const uri = `${baseApi}delete?url=${encoded}`;

        fetch(uri, {
            method: "DELETE",
            mode: "cors",
            cache: "no-cache",
            credentials: "same-origin",
            headers: {
                "X-Requested-With": "XMLHttpRequest",
                "RequestVerificationToken": antiforgeryFormField.value,
            },
        })
            .then((result) => {
                if (!result.ok) {
                    throw Error(result.statusText);
                }
                if (row.isBlob || row.isContainer) {
                    search();
                } else if (row.isDirectory) {
                    if (path) {
                        setPath(null);
                    } else {
                        search();
                    }
                }
            })
            .catch((err) => {
                console.log(err);
            });
    };

    const handleBreadCrumbClick = (index: number, value: string) => {
        if (index === 0) {
            setContainer(null);
            setPath(null);
        } else if (index === 1) {
            setContainer(value);
            setPath(null);
        } else {
            setPath(value);
        }
    };

    const handleCreateContainerClick = () => {
        fetch(`${baseApi}createcontainer?container=${createContainerName}`, {
            method: "POST",
            mode: "cors",
            cache: "no-cache",
            credentials: "same-origin",
            headers: {
                "X-Requested-With": "XMLHttpRequest",
                "RequestVerificationToken": antiforgeryFormField.value,
            },
        }).then((result) => {
            if (!result.ok) {
                throw Error(result.statusText);
            }
            setContainer(createContainerName);
        });
    };

    const handleUploadFileClick = () => {
        const formData = new FormData();
        formData.append("postedFiles", file ?? "");
        let uri = `${baseApi}upload?container=${container}`;
        if (path) {
            uri = uri + `?path=${path}`;
        }
        fetch(uri, {
            method: "POST",
            mode: "cors",
            cache: "no-cache",
            credentials: "same-origin",
            headers: {
                "X-Requested-With": "XMLHttpRequest",
                "RequestVerificationToken": antiforgeryFormField.value,
            },
            body: formData,
        })
            .then((res) => res.json())
            .then((result) => {
                setContainer(result.container);
                setPath(result.path);
                setFile(null);
            })
            .catch((err) => {
                console.log(err);
            });
    };

    const renderForm = () => {
        if (!path && !container) {
            return (
                <>
                    <div className="explorer-form">
                        <label htmlFor="txtContainer">
                            <FormattedMessage id="newContainer" />
                        </label>
                        <input type="text" onChange={(e) => setCreateContainerName(e.target.value ?? null)} />
                        <button type="button" onClick={handleCreateContainerClick} className="btn btn-primary">
                            <FormattedMessage id="submit" />
                        </button>
                    </div>
                </>
            );
        } else {
            return (
                <>
                    <div className="explorer-form">
                        <label htmlFor="txtContainer">
                            <FormattedMessage id="uploadNewFile" />
                        </label>
                        <input type="file" onChange={(e) => setFile(e.target.files?.item(0) ?? null)} />
                        <button type="button" onClick={handleUploadFileClick} className="btn btn-primary">
                            <FormattedMessage id="submit" />
                        </button>
                    </div>
                </>
            );
        }
    };

    const renderBreadCrumbs = () => {
        return (
            <>
                <ul className="breadcrumb">
                    {Object.keys(breadCrumbs ?? {}).map((item, i) => (
                        <li key={i}>
                            {breadCrumbs[item] && (
                                <a onClick={() => handleBreadCrumbClick(i, breadCrumbs[item])}>{item}</a>
                            )}
                            {!breadCrumbs[item] && <span>{item}</span>}
                        </li>
                    ))}
                </ul>
            </>
        );
    };

    if (error) {
        return <div>Error: {error.message}</div>;
    } else {
        return (
            <>
                <div>
                    {renderBreadCrumbs()}
                    {renderForm()}

                    <DataTable
                        className="table"
                        columns={columns}
                        data={data ?? []}
                        progressPending={!isLoaded}
                        pagination
                        paginationRowsPerPageOptions={[25, 50, 100]}
                    />
                </div>
            </>
        );
    }
};

export default App;

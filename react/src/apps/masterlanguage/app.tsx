import { FormEvent, FunctionComponent, useEffect, useState } from "react";
import DataTable, { TableColumn } from "react-data-table-component";
import { FormattedMessage } from "react-intl";
import "./app.scss";
import "rc-tree/assets/index.css";
import { ErrorMessage } from "apps/errorMessage";
import Tree from "rc-tree";
import { DataNode, EventDataNode } from "rc-tree/lib/interface";

export interface LanguageResult {
    name: string;
    newMaster: string;
    oldMaster: string;
    oldMasterStatus: string;
    url: string;
}

export interface optiTreeNode {
    title: string;
    key: string;
    url: string;
    children: optiTreeNode[] | null;
}

export interface LanguageItem {
    languageID: string;
    name: string;
}
const App: FunctionComponent = () => {
    const [message, setMessage] = useState<string | null>(null);
    const [languages, setLanguages] = useState<LanguageItem[] | null>([]);
    const [targetLanguage, setTargetLanguage] = useState<string | null>(null);
    const [contentLink, setContentLink] = useState<string | null>(null);
    const [switchOnly, setSwitchOnly] = useState<boolean>(true);
    const [recursive, setRecursive] = useState<boolean>(false);
    const [treeData, setTreeData] = useState<DataNode[] | undefined>(undefined);
    const baseApi = window.location.protocol + "//" + window.location.host + "/api/masterlanguage/";
    const root = document.getElementById("root") as HTMLElement;
    const antiforgeryFormFieldName: string = root.dataset.epiAntiforgeryFormFieldName as string;
    const antiforgeryFormField: HTMLInputElement = document.getElementsByName(
        antiforgeryFormFieldName
    )[0] as HTMLInputElement;

    

    const getLanguages = () => {
        fetch(`${baseApi}languages`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
            },
        })
            .then(
                (res) => res.json(),
                (error) => {
                    setMessage(error);
                }
            )
            .then((result: LanguageItem[]) => {
                setLanguages(result);
                setTargetLanguage(result[0]?.languageID ?? null);
            });
    };

    const handleSubmit = (e: FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        const body = {
            contentId: parseInt(contentLink ?? "0"),
            targetLanguage: targetLanguage,
            processChildren: recursive,
            switchOnly: switchOnly,
        };
        fetch(`${baseApi}SwitchMasterLanguage`, {
            method: "POST",
            body: JSON.stringify(body),
            headers: {
                "Content-Type": "application/json",
                Accept: "application/json",
                RequestVerificationToken: antiforgeryFormField.value,
            },
        })
            .then(
                (res) => res.json(),
                (error) => {
                    setMessage(error);
                }
            )
            .then((result) => {
                if (result?.status && result.status == 500) {
                    setMessage(result.detail);
                } else {
                    setMessage(result);
                }
            });
    };

    const onSelect = (
        selectedKeys: React.Key[],
        info: {
            event: "select";
            selected: boolean;
            node: EventDataNode<DataNode>;
            selectedNodes: DataNode[];
            nativeEvent: MouseEvent;
        }
    ) => {
        setContentLink(info.selectedNodes[0].key.toString());
    };

    const getTreeNode = (id: string, data: DataNode[]): DataNode | null => {
        for (var i = 0; i < data.length; i++) {
            if (data[i].key === id) {
                return data[i];
            }
            if ((data[i].children?.length ?? 0) > 0) {
                const childFound = getTreeNode(id, data[i].children ?? []);
                if (childFound) {
                    return childFound;
                }
            }
        }
        return null;
    };

    const onLoadData = (treeNode: EventDataNode<DataNode>) => {
        console.log("load data...");
        return getChildren(treeNode.key as string)
            .then(
                (res) => res.json(),
                (error) => {
                    setMessage(error);
                }
            )
            .then((result) => {
                const clone = [...(treeData ?? [])];
                const cloneNode = getTreeNode(treeNode.key as string, clone);
                if (cloneNode) {
                    cloneNode.children = result;
                    setTreeData(clone);
                }
            });
    };

    const getChildren = (parentId: string | null) => {
        let url = `${baseApi}getTree`;
        if (parentId) {
            url += `?parentId=${parentId}`;
        }

        return fetch(url);
    };

    useEffect(() => {
        getLanguages();
        getChildren(null)
            .then(
                (res) => res.json(),
                (error) => {
                    setMessage(error);
                }
            )
            .then((result) => {
                setTreeData(result);
            });
    }, []);

    return (
        <>
            <main className="page">
                <div className="card">
                    <div className="card-header">
                        <div className="card-title">
                            <FormattedMessage id="masterLanguageSwitcher" />
                        </div>
                    </div>
                    <div className="card-body">
                        <form method="post" onSubmit={(e) => handleSubmit(e)}>
                            <div className="form-row">
                                <div className="form-group w-2col">
                                    <label htmlFor="validFrom">
                                        <FormattedMessage id="contentToProcess" />
                                    </label>
                                    <Tree onSelect={onSelect} loadData={onLoadData} treeData={treeData} />
                                </div>
                                <div className="form-group w-2col ">
                                    <label htmlFor="targetLanguage">
                                        <FormattedMessage id="targetLanguage" />
                                    </label>
                                    <select
                                        className="form-control"
                                        id="targetLanguage"
                                        name="targetLanguage"
                                        onChange={(e) => {
                                            setTargetLanguage(e.currentTarget.value);
                                        }}
                                    >
                                        {languages?.map((l) => (
                                            <option key={l.languageID} value={l.languageID}>
                                                {l.name}
                                            </option>
                                        ))}
                                        ;
                                    </select>
                                </div>
                            </div>
                            <div className="form-row">
                                <div className="form-group w-2col ">
                                    <label htmlFor="switchMode">
                                        <FormattedMessage id="switchMode" />
                                    </label>
                                    <label className="checkbox">
                                        <input
                                            className="form-control"
                                            value="justSwitch"
                                            name="switchMode"
                                            type="radio"
                                            defaultChecked={true}
                                            onChange={(e) => {
                                                setSwitchOnly(e.target.value === "justSwitch");
                                            }}
                                        />
                                        <span>
                                            <FormattedMessage id="justSwitch" />
                                        </span>
                                    </label>
                                    <label className="checkbox">
                                        <input
                                            className="form-control"
                                            value="convertOrCreate"
                                            name="switchMode"
                                            type="radio"
                                            onChange={(e) => {
                                                setSwitchOnly(e.target.value === "justSwitch");
                                            }}
                                        />
                                        <FormattedMessage id="convertOrCreate" />
                                    </label>
                                </div>
                                <div className="form-group w-2col ">
                                    <label htmlFor="validFrom">
                                        <FormattedMessage id="processChildren" />
                                    </label>
                                    <label className="checkbox">
                                        <input
                                            className="form-control"
                                            id="processChildren"
                                            name="processChildren"
                                            type="checkbox"
                                            onChange={(e) => {
                                                setRecursive(e.target.checked);
                                            }}
                                        />
                                        <FormattedMessage id="recursiveChildren" />
                                    </label>
                                </div>
                            </div>
                            <button type="submit" className="btn-primary">
                                <FormattedMessage id="changeLanguage" />
                            </button>
                        </form>
                        <div className="results">
                            {message && <p>{message}</p>}
                        </div>
                    </div>
                </div>
            </main>
        </>
    );
};

export default App;

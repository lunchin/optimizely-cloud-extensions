import { FormEvent, FunctionComponent, useEffect, useState } from "react";
import DataTable, { TableColumn } from "react-data-table-component";
import { FormattedMessage } from "react-intl";
import { useParams } from "react-router-dom";
import { FiDelete, FiSave } from "react-icons/fi";
import { ErrorMessage } from "apps/errorMessage";

export interface PromotionCoupons {
    coupons: CouponItem[];
    promotionName: string;
    promotionId: number;
    maxRedemptions: number;
}
export interface CouponItem {
    code: string;
    created: string;
    validFrom: string;
    expiration: string;
    maxRedemptions: number;
    usedRedemptions: number;
    id: number;
    promotionId: number;
}

const EditCoupoons: FunctionComponent = () => {
    const [error, setError] = useState<ErrorMessage | null>(null);
    const [data, setData] = useState<PromotionCoupons | null>(null);
    const [isLoaded, setIsLoaded] = useState(false);
    const baseApi = window.location.protocol + "//" + window.location.host + "/api/multicoupons/";
    const root = document.getElementById("root") as HTMLElement;
    const antiforgeryFormFieldName: string = root.dataset.epiAntiforgeryFormFieldName as string;
    const antiforgeryFormField: HTMLInputElement = document.getElementsByName(
        antiforgeryFormFieldName
    )[0] as HTMLInputElement;

    const { id } = useParams();
    const columns: TableColumn<CouponItem>[] = [
        {
            name: <FormattedMessage id="code" />,
            cell: (row) =>
                <>
                    <input name={"hiddenId_" + row.id.toString()} type="hidden" defaultValue={row.id} />
                    <input name={"hiddenPromotionId_" + row.promotionId.toString()} type="hidden" defaultValue={row.promotionId} />
                    <input name={"code_" + row.code} defaultValue={row.code} className="form-control" />
                </>,
            grow: 2
        },
        {
            name: <FormattedMessage id="created" />,
            selector: (row) => new Date(row.created).toLocaleDateString(),
        },
        {
            name: <FormattedMessage id="validFrom" />,
            cell: (row) => <input name={"validFrom_" + row.id.toString()} defaultValue={formatDate(new Date(row.validFrom))} className="form-control" type="Date" />,
        },
        {
            name: <FormattedMessage id="expiration" />,
            cell: (row) => <input name={"expiration_" + row.id.toString()} defaultValue={formatDate(new Date(row.expiration))} className="form-control" type="Date" />,
        },
        {
            name: <FormattedMessage id="maxRedemptions" />,
            cell: (row) => <input name={"maxRedemptions_" + row.id.toString()} defaultValue={row.maxRedemptions.toString()} className="form-control" type="number" />,
        },
        {
            name: <FormattedMessage id="usedRedemptions" />,
            cell: (row) => <input name={"usedRedemptions_" + row.id.toString()} defaultValue={row.usedRedemptions.toString()} className="form-control" type="number" />,
        },
        {
            name: <FormattedMessage id="actions" />,
            cell: () => <>
                <a className="actions-icon" onClick={(e) => updateCoupon(e)}>
                    <FiSave />
                </a>
                <a onClick={(e) => deleteCoupon(e)}>
                    <FiDelete />
                </a>
            </>,
        },
    ];

    useEffect(() => {
        load();
    }, []);

    const load = () => {
        const url = `${baseApi}coupons?id=${id}`;

        fetch(url)
            .then((res) => res.json())
            .then(
                (result) => {
                    setIsLoaded(true);
                    setData(result);
                },
                (error) => {
                    setIsLoaded(true);
                    setError(error);
                }
            );
    };

    const formatDate = (d: Date) => {
        let month = '' + (d.getMonth() + 1),
            day = '' + d.getDate();

        if (month.length < 2) {
            month = '0' + month;
        }
        if (day.length < 2) {
            day = '0' + day;
        }
        return [d.getFullYear().toString(), month, day].join('-');
    }

    const getFormData = (form: HTMLFormElement | null, rowData: HTMLDivElement | null, actionType: string) => {

        if (form == null || rowData == null) {
            return null;
        }
        type formRecord = Record<string, string | null>

        const record: formRecord = {
            id: (rowData.querySelector('input[name^="hiddenId"]') as HTMLInputElement)?.value,
            promotionId: (rowData.querySelector('input[name^="hiddenPromotionId"]') as HTMLInputElement)?.value,
            code: (rowData.querySelector('input[name^="code"]') as HTMLInputElement)?.value,
            validFrom: (rowData.querySelector('input[name^="validFrom"]') as HTMLInputElement)?.value,
            expiration: (rowData.querySelector('input[name^="expiration"]') as HTMLInputElement)?.value,
            maxRedemptions: (rowData.querySelector('input[name^="maxRedemptions"]') as HTMLInputElement)?.value,
            usedRedemptions: (rowData.querySelector('input[name^="usedRedemptions"]') as HTMLInputElement)?.value,
        }

        var params = new URLSearchParams();
        for (var key in record) {
            if (record[key]) {
                params.append(key, record[key] ?? '');
            }
        }
        params.append("__RequestVerificationToken", antiforgeryFormField.value);
        params.append("actionType", actionType);

        return params;
    }

    const updateCoupon = (e: React.MouseEvent<HTMLAnchorElement>) => {
        e.preventDefault();
        let link = e.target as HTMLAnchorElement;
        const form = link.closest('form');
        const rowData = link.closest('[role="row"]') as HTMLDivElement;
        const formData = getFormData(form, rowData, "update");
        if (!formData) {
            return;
        }

        fetch(`${form?.action}`, {
            method: form?.method,
            body: formData.toString(),
            headers: {
                "Content-Type": "application/x-www-form-urlencoded",
            }
        })
            .then(
                (res) => {
                    if (res.ok) {
                        load();
                    }
                },
                (error) => {
                    setIsLoaded(true);
                    setError(error);
                }
            );
    };

    const deleteCoupon = (e: React.MouseEvent<HTMLAnchorElement>) => {
        e.preventDefault();
        let link = e.target as HTMLAnchorElement;
        const form = link.closest('form');
        const rowData = link.closest('[role="row"]') as HTMLDivElement;
        const formData = getFormData(form, rowData, "delete");

        if (!formData) {
            return;
        }

        if (confirm("Do you really want to delete this coupon?")) {
            fetch(`${form?.action}`, {
                method: form?.method,
                body: formData.toString(),
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded",
                },
            })
                .then((res) => {
                    if (res.ok) {
                        load();
                    }
                },
                    (error) => {
                        setIsLoaded(true);
                        setError(error);
                    }
                );
        }
    }

    const generateCoupons = (e: FormEvent<HTMLFormElement>) => {
        // Prevent the browser from reloading the page
        e.preventDefault();

        // Read the form data
        const form = e.target as HTMLFormElement;
        const formData = new FormData(form);
        const body = {
            validFrom: formData.get('validFrom'),
            promotionId: id,
            expiration: formData.get('expiration'),
            quantity: Number(formData.get('quantity') ?? '0'),
            maxRedemptions: Number(formData.get('maxRedemptions') ?? '0')
        }

        // You can pass formData as a fetch body directly:
        fetch(`${baseApi}generateCoupon`, {
            method: form.method,
            body: JSON.stringify(body),
            headers: {
                "Content-Type": "application/json",
                "Accept": "application/json",
                "RequestVerificationToken": antiforgeryFormField.value,
            },
        })
            .then((res) => res.json())
            .then(
                (result) => {
                    if (result) {
                        load();
                    }
                },
                (error) => {
                    setIsLoaded(true);
                    setError(error);
                }
            );
    };

    const downloadAll = (e: React.MouseEvent<HTMLButtonElement>) => {
        // Prevent the browser from reloading the page
        e.preventDefault();

        const body = {
            promotionId: id,
        }

        // You can pass formData as a fetch body directly:
        fetch(`${baseApi}downloadCoupons`, {
            method: "POST",
            body: JSON.stringify(body),
            headers: {
                "Content-Type": "application/json",
                "Accept": "application/json",
                "RequestVerificationToken": antiforgeryFormField.value,
            },
        })
            .then((res) => res.blob(),
                (error) => {
                    setIsLoaded(true);
                    setError(error);
                })
            .then(blob => {
                if (!blob) {
                    return;
                }
                const aElement = document.createElement("a");
                aElement.setAttribute("download", `couponsexport_${new Date().toISOString()}.csv`);
                const href = URL.createObjectURL(blob);
                aElement.href = href;
                aElement.setAttribute("target", "_blank");
                aElement.click();
                URL.revokeObjectURL(href);
                aElement.remove();
            });
    };

    const deleteAll = (e: React.MouseEvent<HTMLButtonElement>) => {
        // Prevent the browser from reloading the page
        e.preventDefault();

        const body = {
            promotionId: id,
        }

        // You can pass formData as a fetch body directly:
        fetch(`${baseApi}deleteAllCoupons`, {
            method: "POST",
            body: JSON.stringify(body),
            headers: {
                "Content-Type": "application/json",
                "Accept": "application/json",
                "RequestVerificationToken": antiforgeryFormField.value,
            },
        })
            .then(
                (res) => {
                    if (res.ok) {
                        load();
                    }
                },
                (error) => {
                    setIsLoaded(true);
                    setError(error);
                }
            );
    };

    if (error) {
        return <div>Error: {error.message}</div>;
    } else {
        return (
            <>
                <main className="page">
                    <h3><FormattedMessage id="manageCoupons" />{data?.promotionName}</h3>
                    <div className="card">
                        <div className="card-header">
                            <div className="card-title"><FormattedMessage id="generateCoupons" /></div>
                        </div>
                        <div className="card-body">
                            <form method="post" onSubmit={(e) => generateCoupons(e)}>
                                <div className="form-row">
                                    <div className="form-group w-4col ">
                                        <label htmlFor="validFrom"><FormattedMessage id="validFrom" /></label>
                                        <input className="form-control" id="validFrom" name="validFrom" type="date" />
                                    </div>
                                    <div className="form-group w-4col ">
                                        <label htmlFor="validFrom"><FormattedMessage id="expiration" /></label>
                                        <input className="form-control" id="expiration" name="expiration" type="date" />
                                    </div>
                                    <div className="form-group w-4col ">
                                        <label htmlFor="quantity"><FormattedMessage id="quantity" /></label>
                                        <input className="form-control" id="quantity" name="quantity" type="number" />
                                    </div>
                                    <div className="form-group w-4col ">
                                        <label htmlFor="validFrom"><FormattedMessage id="maxRedemptions" /></label>
                                        <input className="form-control" id="maxRedemptions" name="maxRedemptions" type="number" />
                                    </div>
                                </div>
                                <button type="submit" className="btn-primary">
                                    <FormattedMessage id="generate" />
                                </button>
                            </form>
                        </div>
                    </div>
                    <div className="bulkActions">
                        <button type="button" className="btn-primary" onClick={(e) => downloadAll(e)}><FormattedMessage id="download" /></button>
                        <button type="button" className="btn-primary" onClick={(e) => deleteAll(e)}><FormattedMessage id="deleteAll" /></button>
                    </div>
                </main>
                <div className="coupons">
                    <form method="post" action="/api/multicoupons/updateOrDeleteCoupon">
                        <DataTable
                            title={<FormattedMessage id="generatedCoupons" />}
                            columns={columns}
                            data={data?.coupons ?? []}
                            progressPending={!isLoaded}
                        />
                    </form>
                </div>
            </>
        );
    }
};

export default EditCoupoons;

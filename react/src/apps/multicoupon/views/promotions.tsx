import { ErrorMessage } from "apps/errorMessage";
import { FunctionComponent, useEffect, useState } from "react";
import DataTable, { TableColumn } from "react-data-table-component";
import { FormattedMessage } from "react-intl";
import { Link } from "react-router-dom";

export interface PromotionItem {
    name: string;
    id: number;
    discountType: string;
    isActive: boolean;
}

const Promotions: FunctionComponent = () => {
    const [error, setError] = useState<ErrorMessage | null>(null);
    const [data, setData] = useState([]);
    const [isLoaded, setIsLoaded] = useState(false);
    const [totalRows, setTotalRows] = useState(0);
    const [perPage, setPerPage] = useState(25);
    const baseApi = window.location.protocol + "//" + window.location.host + "/api/multicoupons/";

    const columns: TableColumn<PromotionItem>[] = [
        {
            name: <FormattedMessage id="name" />,
            cell: (row) => <Link to={`/edit/${row.id}`}>{row.name}</Link>,
            grow: 2,
        },
        {
            name: <FormattedMessage id="discountType" />,
            selector: (row) => row.discountType,
        },
        {
            name: <FormattedMessage id="discountStatus" />,
            selector: (row) => row.isActive ? "active" : "inactive",
        },
    ];

    useEffect(() => {
        search(1);
    }, [perPage]);

    const search = (page: number) => {
        const url = `${baseApi}promotions?page=${page}&pageSize=${perPage}`;

        fetch(url)
            .then((res) => res.json())
            .then(
                (result) => {
                    setIsLoaded(true);
                    setData(result.promotions);
                    setTotalRows(result.total);
                },
                (error) => {
                    setIsLoaded(true);
                    setError(error);
                }
            );
    };

    const handlePageChange = (page: number) => {
        search(page);
    };

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const handlePerRowsChange = async (newPerPage: number, _page: number) => {
        setPerPage(newPerPage);
    };

    if (error) {
        return <div>Error: {error.message}</div>;
    } else {
        return (
            <>
                <div className="promotions">
                    <DataTable
                        title="Promotions"
                        columns={columns}
                        data={data}
                        progressPending={!isLoaded}
                        pagination
                        paginationServer
                        paginationTotalRows={totalRows}
                        paginationRowsPerPageOptions={[25, 50, 100]}
                        onChangeRowsPerPage={handlePerRowsChange}
                        onChangePage={handlePageChange}
                    />
                </div>
                ;
            </>
        );
    }
};

export default Promotions;

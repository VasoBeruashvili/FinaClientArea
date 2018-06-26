function GenerateFullDate(originalDate) {
    return originalDate.getDate() + "-" + (originalDate.getMonth() + 1) + "-" + originalDate.getFullYear();
}

function GenerateFullDateTime(originalDate) {
    return originalDate.getDate() + "-" + (originalDate.getMonth() + 1) + "-" + originalDate.getFullYear() + " " + originalDate.getHours() + ":" + originalDate.getMinutes() + ":" + originalDate.getSeconds();
}

var app = angular.module('privateCabinet', ['ui.bootstrap', 'angularGrid']);

app.controller('generalController', ['$scope', '$http', '$window', function ($scope, $http, $window) {
    $http.get('/Home/GetCurrentUser').then(function (response) {
        $scope.contragentName = response.data.root.UserName;
    });

    $scope.LogoutContragent = function () {
        window.localStorage.setItem('login', false);
        window.location = '/Login/Logout';
    }

    $scope.RedirectToRequests = function () {
        $window.location = 'http://localhost:2086/Home/Requests';
    }

    $scope.RedirectToLicenses = function () {
        $window.location = 'http://localhost:2086/Home/Licenses';
    }

    $scope.RedirectToInvoices = function () {
        $window.location = 'http://localhost:2086/Home/Invoices';
    }

    $scope.RedirectToNews = function () {
        $window.location = 'http://localhost:2086/Home/News';
    }
}]);

app.controller('requestsController', ['$scope', '$http', '$window', function ($scope, $http, $window) {
    var columnDefs = [
        {
            headerName: "",
            width: 10,
            field: "GeneralDoc_StatusID",
            cellClassRules: {
                'status-1': 'x === 1',
                'status-2': 'x === 2',
                'status-3': 'x === 3',
                'status-4': 'x === 4',
                'status-5': 'x === 5'
            },
            suppressMenu: true,
            suppressSorting: true,
            suppressResize: true
        },
        { headerName: "გაფორმების თარიღი", field: "GeneralDoc_Tdate", width: 165 },
        { headerName: "საკითხი", field: "GeneralDoc_Purpose", width: 415, filter: 'text' },
        { headerName: "ტიპი", field: "CRMIncidentType_Name", width: 180 },
        { headerName: "დაწყების თარიღი", field: "StartDate", width: 175 },
        { headerName: "დასრულების თარიღი", field: "EndDate", width: 175 }
    ];

    $scope.gridOptions = {
        columnDefs: columnDefs,
        enableFilter: true,
        rowData: null,
        enableColResize: true,
        rowSelection: 'single',
        enableSorting: true
    };

    $http.get('/Home/GetCurrentUser').then(function (response) {
        $scope.contragentName = response.data.root.UserName;
    });

    $scope.init = function () {
        $http.get('/Home/GetCRMIncidents').then(function (response) {
            angular.forEach(response.data.root, function (crmIncident) {
                var GeneralDoc_Tdate = new Date(parseInt(crmIncident.GeneralDoc_Tdate.substr(6)));
                crmIncident.GeneralDoc_Tdate = GenerateFullDateTime(GeneralDoc_Tdate);

                var StartDate = new Date(parseInt(crmIncident.StartDate.substr(6)));
                crmIncident.StartDate = GenerateFullDateTime(StartDate);

                if (crmIncident.EndDate !== null) {
                    var EndDate = new Date(parseInt(crmIncident.EndDate.substr(6)));
                    crmIncident.EndDate = GenerateFullDateTime(EndDate);
                }
            });

            $scope.gridOptions.rowData = response.data.root;
            $scope.gridOptions.api.onNewRows();
        });
    };

    $scope.LogoutContragent = function () {
        window.localStorage.setItem('login', false);
        window.location = '/Login/Logout';
    }
}]);

app.controller('licensesController', ['$scope', '$http', '$window', function ($scope, $http, $window) {
    var columnDefs = [
        { headerName: "მოდული", field: "PackageName", width: 290, filter: 'text' },
        { headerName: "ლიცენზიის გასაღები", field: "key", width: 290, filter: 'text' },
        { headerName: "კომენტარი", field: 'Comment', width: 340, filter: 'text', editable: true },
        {
            headerName: "გაუქმების თარიღი", field: "tdate", width: 150,
            cellClassRules: {
                'perpetualLicense': 'x === "უვადო"'
            }
        },
        {
            headerName: "",
            field: "",
            width: 50,
            cellRenderer: function (params) {
                return "<img src='../../Content/Resources/deactivateKey18.png' style='cursor: pointer; display: block; margin-left: auto; margin-right: auto; margin-top: 3px;' title='ლიცენზიის დაბრუნება' />";
            },
            suppressMenu: true,
            suppressSorting: true,
            suppressResize: true
        }
    ];

    $scope.gridOptions = {
        columnDefs: columnDefs,
        enableFilter: true,
        rowData: null,
        enableColResize: true,
        rowSelection: 'single',
        enableSorting: true,
        cellValueChanged: function (node) {
            if (node.newValue !== node.oldValue && node.newValue.length <= 200) {
                $http.post('/Home/UpdateKeyComment', { key: node.data.key, comment: node.newValue }).then(function (response) {
                });
            }
        },
        cellClicked: function (cell) {
            if (cell.colDef.field === '') {
                $http.post('/Home/DeactivateKey', { key: cell.data.key }).then(function (response) {
                    if (response.data.root) {
                        alert("ლიცენზია წარმატებით დაბრუნდა სერვერზე.")
                    } else {
                        alert("ლიცენზია ვერ დაბრუნდა სერვერზე!");
                    }
                });
            }            
        }
    };

    $http.get('/Home/GetCurrentUser').then(function (response) {
        $scope.contragentName = response.data.root.UserName;
    });

    $scope.init = function () {
        $http.get('/Home/GetContragentLicenses').then(function (response) {
            $scope.gridOptions.rowData = response.data.root;
            $scope.gridOptions.api.onNewRows();
        });
    };

    $scope.LogoutContragent = function () {
        window.localStorage.setItem('login', false);
        window.location = '/Login/Logout';
    }
}]);

app.controller('invoicesController', ['$scope', '$http', '$window', function ($scope, $http, $window) {
    var columnDefs = [
        { headerName: "თარიღი", field: "TDate", width: 180 },
        { headerName: "შინაარსი", field: "Purpose", width: 730, filter: 'text'},
        { headerName: "თანხა", field: "Amount", width: 160, filter: 'number' },
        {
            headerName: "",
            field: "",
            width: 50,
            cellRenderer: function (params) {
                return "<img src='../../Content/Resources/printer18.png' style='cursor: pointer; display: block; margin-left: auto; margin-right: auto; margin-top: 3px;' title='ინვოისის ბეჭდვა' />";
            },
            suppressMenu: true,
            suppressSorting: true,
            suppressResize: true
        }
    ];

    $scope.gridOptions = {
        columnDefs: columnDefs,
        enableFilter: true,
        rowData: null,
        enableColResize: true,
        rowSelection: 'single',
        enableSorting: true,
        cellClicked: function (cell) {
            if (cell.colDef.field === '') {
                $scope.GeneratePDF(cell.data.ID);
            }
        }
    };

    $http.get('/Home/GetCurrentUser').then(function (response) {
        $scope.contragentName = response.data.root.UserName;
    });

    $scope.init = function () {
        $http.get('/Home/GetContragentInvoices').then(function (response) {
            angular.forEach(response.data.root, function (invoice) {
                var invoiceDate = new Date(parseInt(invoice.TDate.substr(6)));
                invoice.TDate = GenerateFullDateTime(invoiceDate);
            });

            $scope.gridOptions.rowData = response.data.root;
            $scope.gridOptions.api.onNewRows();
        });
    };

    $scope.GeneratePDF = function (invoiceID) {
        window.open('http://localhost:2086/Home/GeneratePDF?invoiceID=' + invoiceID, '_blank');
    }

    $scope.LogoutContragent = function () {
        window.localStorage.setItem('login', false);
        window.location = '/Login/Logout';
    }
}]);

app.controller('newsController', ['$scope', '$http', '$window', function ($scope, $http, $window) {
    $http.get('/Home/GetCurrentUser').then(function (response) {
        $scope.contragentName = response.data.root.UserName;
    });

    $scope.init = function () {
        $http.get('/Home/GetNews').then(function (response) {
            angular.forEach(response.data.root.NewsItems, function (newsItem) {
                var newsItemDate = new Date(parseInt(newsItem.TDate.substr(6)));
                newsItem.TDate = GenerateFullDate(newsItemDate);
            });

            $scope.newsItems = response.data.root.NewsItems;
        });
    };

    $scope.LogoutContragent = function () {
        window.localStorage.setItem('login', false);
        window.location = '/Login/Logout';
    }
}]);
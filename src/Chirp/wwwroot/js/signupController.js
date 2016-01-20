(function () {
    "use strict";

    //Getting the existing module
    angular.module("app-signup")
        .controller("signupController", signupController);

    function signupController($http, $window) {
        var vm = this;

        vm.newUser = {};

        vm.message = "";
        vm.isBusy = false;

        vm.signup = function () {
            vm.message = "";
            vm.isBusy = true;

            $http.post("/auth/signup", vm.newUser)
                .then(function (response) {
                    //Success
                    $window.location.href = response.data.url;
                }, function (error) {
                    //Failure
                    vm.message = "Failed to signup: " + error;
                    vm.newUser.Password = "";
                    vm.newUser.ConfirmPassword = "";
                })
                .finally(function () {
                    vm.isBusy = false;
                });
        };
    }
})();
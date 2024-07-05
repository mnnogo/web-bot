var connection = new signalR.HubConnectionBuilder().withUrl("/messageHub").build();

connection.on("ThrowServerError", function (message) {
    alert("smh");
    throw new Error(message);
});
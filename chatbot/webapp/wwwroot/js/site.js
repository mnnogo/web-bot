var connection = new signalR.HubConnectionBuilder().withUrl("/messageHub").build();

connection.on("ReceiveMessage", function (message) {
    addMessage("Bot", message, "text-success");
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var message = document.getElementById("inputText").value;
    addMessage("User", message, "text-primary");

    connection.invoke("SendMessage", message, connection.connectionId).catch(function (err) {
        return console.error(err.toString());
    });
    document.getElementById("inputText").value = '';
    event.preventDefault();
});

function addMessage(sender, message, textColor) {
    var div = document.createElement("div");
    div.classList.add("d-flex", "align-items-start", "mb-2");

    var badge = document.createElement("span");
    badge.classList.add("badge", "badge-pill", textColor, "mr-2");
    badge.textContent = sender;

    var msg = document.createElement("div");
    msg.classList.add("bg-light", "p-2", "rounded", "flex-grow-1");
    msg.textContent = message;

    div.appendChild(badge);
    div.appendChild(msg);
    document.getElementById("messagesList").appendChild(div);
    document.getElementById("messagesList").scrollTop = document.getElementById("messagesList").scrollHeight;
}
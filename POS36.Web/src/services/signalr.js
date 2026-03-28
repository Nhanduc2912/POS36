import * as signalR from "@microsoft/signalr";

class SignalRService {
  constructor() {
    this.connection = null;
  }

  startConnection() {
    if (this.connection) return;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl("http://:5198/kitchenHub", {
        accessTokenFactory: () => localStorage.getItem("token"),
      })
      .withAutomaticReconnect()
      .build();

    this.connection
      .start()
      .then(() => console.log("SignalR Connected."))
      .catch((err) => console.error("SignalR Connection Error: ", err));
  }

  on(eventName, callback) {
    if (!this.connection) this.startConnection();
    this.connection.on(eventName, callback);
  }

  off(eventName) {
    if (this.connection) {
      this.connection.off(eventName);
    }
  }

  stopConnection() {
    if (this.connection) {
      this.connection.stop();
      this.connection = null;
    }
  }
}

export default new SignalRService();

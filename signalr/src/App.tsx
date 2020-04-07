import React from 'react';
import logo from './logo.svg';
import './App.css';
import {HubConnectionBuilder, LogLevel, HttpTransportType} from '@aspnet/signalr';


function App() {
  const connection = new HubConnectionBuilder()
      .withUrl("http://localhost:7071/api/v1.0/messages/binding",{ transport : HttpTransportType.WebSockets, accessTokenFactory : ()=>"token_test"})
      
      .configureLogging(LogLevel.Information)
      
      .build();


    

    connection.on("send", data => {
        console.log(data, "el famoso cristian");
    });
     
    connection.start()
        .then(() => {
            console.log("start");
            connection.send("SendMessage", "Hello");
            console.log("invoke");

        });


  return (
    <div className="App">
      <header className="App-header">
        <img src={logo} className="App-logo" alt="logo" />
        <p>
          Edit <code>src/App.tsx</code> and save to reload.
        </p>
        <a
          className="App-link"
          href="https://reactjs.org"
          target="_blank"
          rel="noopener noreferrer"
        >
          Learn React
        </a>
      </header>
    </div>
  );
}

export default App;

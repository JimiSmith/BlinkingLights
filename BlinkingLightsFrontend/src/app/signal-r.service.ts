import { Injectable, ChangeDetectorRef, ApplicationRef } from '@angular/core';
import { HubConnectionBuilder, HubConnection } from '@aspnet/signalr';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  host = 'http://localhost:7071';
  connection: HubConnection;

  constructor(private applicationRef: ApplicationRef) {
    this.connection = new HubConnectionBuilder()
      .withUrl(this.host + '/notifications')
      .build();
    this.connection
      .start()
      .then(function() {
        console.log('connected');
      })
      .catch(function(error) {
        console.error(error.message);
      });
  }

  on(method: string): Observable<any> {
    return new Observable(subscriber => {
      const listenFunc = (data: any) => {
        subscriber.next(JSON.parse(data));
        // this is needed otherwise change detection doesn't get run. *shrug*
        this.applicationRef.tick();
      };
      this.connection.on(method, listenFunc);
      return () => {
        this.connection.off(method, listenFunc);
      };
    });
  }
}

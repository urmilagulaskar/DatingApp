import { HttpClient} from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';

@Injectable({
  providedIn: 'root'
})

export class MembersService {
  private http = inject(HttpClient);
  baseUrl=environment.apiUrl;

  getMembers(){
    return this.http.get<Member[]>(this.baseUrl + 'users/');
    // if we dont use interceptor, then below code will work
    // with local function for adding auth bearer  
    // return this.http.get<Member[]>(this.baseUrl + 'users/', this.getHttpOptions());
  }

  getMember(username: string){
    return this.http.get<Member>(this.baseUrl + 'users/'+ username);
    //return this.http.get<Member>(this.baseUrl + 'users/'+ username, this.getHttpOptions());
  }

/* local function to get auth bearer. (Note:inject accountservice & import httpHeaders)
 getHttpOptions(){
    return{
      headers: new HttpHeaders({
        Authorization: `Bearer ${this.accountService.currentUser()?.token}`
      })
    }
  } */
}

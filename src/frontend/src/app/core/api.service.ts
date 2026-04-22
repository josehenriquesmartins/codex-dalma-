import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private readonly http: HttpClient) {}

  get<T>(path: string) { return this.http.get<T>(`${environment.apiUrl}${path}`); }
  getBlob(path: string) { return this.http.get(`${environment.apiUrl}${path}`, { responseType: 'blob' }); }
  post<T>(path: string, body: unknown) { return this.http.post<T>(`${environment.apiUrl}${path}`, body); }
  put<T>(path: string, body: unknown) { return this.http.put<T>(`${environment.apiUrl}${path}`, body); }
  delete<T>(path: string) { return this.http.delete<T>(`${environment.apiUrl}${path}`); }
}

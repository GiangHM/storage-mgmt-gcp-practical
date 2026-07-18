//import { environment } from '@/environment/environment.dev';
import axios, { AxiosError, type AxiosResponse } from 'axios';


export default class HttpService {
    baseUrl: string = import.meta.env.VITE_API_BASE_URL;
    CancelToken = axios.CancelToken;
    source: any;
    constructor() {

    }
    

    private handleError = (error: AxiosError) => {
        if (error.response) {
            console.log(error.response.data);
            console.log(error.response.status);
            console.log(error.response.headers);
        } else {
            console.log(error.message);
        }
    };

    private config: any = {
        timeout: 10000,
        withCredentials: true,
        responseType: 'json',
        baseURL: this.baseUrl,
    };

    public async get<T>(route: string): Promise<T> {
        let axiosResponse: AxiosResponse = {} as AxiosResponse;
        await axios.get(route, this.config)
            .then(r => axiosResponse = r)
            .catch(this.handleError);
        return axiosResponse.data;
    }

    public async post<T>(route: string, item: T): Promise<Number> {
        let response: Number = new Number();
        await axios.post(route, item, this.config)
            .then(r => response = r.status)
            .catch(this.handleError);
        return response;
    }

    public async postForData<TRequest, TResponse>(route: string, item: TRequest): Promise<TResponse> {
        let axiosResponse: AxiosResponse = {} as AxiosResponse;
        await axios.post(route, item, this.config)
            .then(r => axiosResponse = r)
            .catch(this.handleError);
        return axiosResponse.data;
    }
}

export default class DocumentCreationRequestModel {
    docTypeCode: string = '';
    docUrl: string = '';
    constructor (docTypeCode: string, docUrl: string){
        this.docTypeCode = docTypeCode;
        this.docUrl = docUrl;
    }
}
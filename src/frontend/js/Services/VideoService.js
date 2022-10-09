
const { BlobServiceClient } = require("@azure/storage-blob");
const axios = require('axios').default;

class VideoService {
    constructor() {
        require('dotenv').config();        
        this.BASE_URL = process.env.BASE_URL;        
        this.FUNCTION_CODE = process.env.FUNCTION_CODE;
    }

    uploadVideo(file) {

    }

    async getVideoJobs() {   
        try {            
            const httpInstance = this.createAxiosInstance('api/GetJobs');
            const response = await httpInstance.get();
            return response.data;
        } catch (error) {
            console.log(error);
        }
    }

    /// Private method
    createAxiosInstance(url) {
        return axios.create({
            baseURL: `${this.BASE_URL}/${url}?code=${this.FUNCTION_CODE}`            
        });        
    }
}

module.exports = VideoService;
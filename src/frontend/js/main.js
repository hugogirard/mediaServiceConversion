// const VideoService = require('./Services/VideoService');
// const videoService = new VideoService();


const { createApp } = Vue;

createApp({
    data(){
        return {
            videoJobs: []
        }
    }
}).mount('#app');

// (function(){
    
//     function reportStatus(message) {
//         const status = document.getElementById("status");
//         status.innerHTML += `${message}<br/>`;
//         status.scrollTop = status.scrollHeight;
//     }
    

//     // DOM LOADED
//     document.addEventListener("DOMContentLoaded", function() {        
//         initialize();        
//     });

//     async function initialize(){
//         reportStatus("Retrieving videos jobs...");
//         const data = await videoService.getVideoJobs();

//         data.forEach(job => {
//             console.log(job);
//         });
//     }

// })();
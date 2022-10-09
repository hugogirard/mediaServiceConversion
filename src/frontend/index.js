//----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
//----------------------------------------------------------------------------------

const { BlobServiceClient } = require("@azure/storage-blob");
const form_data = document.getElementById('form');
const job_name = document.getElementById('name');
const job_description = document.getElementById('description');
const fileInput = document.getElementById('job');
const submitButton = document.getElementById('submit-button');
const status = document.getElementById("status");
const metadata = null;

const reportStatus = message => {
    status.innerHTML += `${message}<br/>`;
    status.scrollTop = status.scrollHeight;
}

// generate blob sas url
const blobSasUrl = process.env.BLOB_SAS_URL;
// Create a new BlobServiceClient
const blobServiceClient = new BlobServiceClient(blobSasUrl);

// Create a unique name for the container by 
// appending the current time to the file name
const containerName = "videos";

// Get a container client from the BlobServiceClient
const containerClient = blobServiceClient.getContainerClient(containerName);
// </snippet_CreateClientObjects>

// list all jobs
// const listBlobs = async () => {
//     fileList.size = 0;
//     fileList.innerHTML = "";
//     try {
//         reportStatus("Retrieving file list...");
//         let iter = containerClient.listBlobsFlat();
//         let blobItem = await iter.next();
//         while (!blobItem.done) {
//             fileList.size += 1;
//             fileList.innerHTML += `<option>${blobItem.value.name}</option>`;


//             blobItem = await iter.next();
//         }
//         if (fileList.size > 0) {
//             reportStatus("Done.");
//         } else {
//             reportStatus("The container does not contain any files.");
//         }
//     } catch (error) {
//         reportStatus(error.message);
//     }
// };

// listButton.addEventListener("click", listFiles);
// </snippet_ListBlobs>

// upload a job
const uploadFiles = async () => {
    try {
        reportStatus("Uploading...");
        const promises = [];
        for (const file of fileInput.files) {
            const blockBlobClient = containerClient.getBlockBlobClient(file.name);
            promises.push(blockBlobClient.uploadBrowserData(file));
        }
        await Promise.all(promises);
        reportStatus("Job uploaded.");
        // listFiles();
    }
    catch (error) {
        reportStatus(error.message);
    }
}

fileInput.addEventListener("change", uploadFiles);

document.addEventListener('DOMContentLoaded', () => {
    form_data.addEventListener('submit', (e) => {
        if(!fileInput.files.length) {
            e.preventDefault();
            return false;
        }
        const input_name = job_name.value;
        const input_desc = job_description.value;
        metadata = {
            name: input_name,
            description: input_desc
        };
        const blockBlobClient = containerClient.getBlockBlobClient(fileInput.files[0].name);
        blockBlobClient.setMetadata(metadata);
    });
});

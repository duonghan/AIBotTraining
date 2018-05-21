using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBotTraining
{
    class Program
    {
        FaceServiceClient faceServiceClient = new FaceServiceClient("ee1588a59d8648fd9129189bb45fd25e", "https://westcentralus.api.cognitive.microsoft.com/face/v1.0");

        public async void CreatePersonGroup(string personGroupId, string personGroupName)
        {
            try
            {
                await faceServiceClient.CreatePersonGroupAsync(personGroupId, personGroupName);
            }
            catch (Exception)
            {
                Console.WriteLine("Lỗi tạo nhóm");
            }
        }

        public async void AddPersonToGroup(string personGroupid, string name, string pathImage)
        {
            try
            {
                await faceServiceClient.GetPersonGroupAsync(personGroupid);
                CreatePersonResult person = await faceServiceClient.CreatePersonAsync(personGroupid, name);

                DetectFaceAndRegister(personGroupid, person, pathImage);
                Console.WriteLine("Thêm thành công");
            }
            catch (Exception e)
            {
                Console.WriteLine("Lỗi thêm thành viên vào nhóm\n" + e.Message);
            }

        }

        private async void DetectFaceAndRegister(string personGroupId, CreatePersonResult person, string pathImage)
        {
            foreach(var imgPath in Directory.GetFiles(pathImage, "*.jpg"))
            {
                using (Stream s = File.OpenRead(imgPath))
                {
                    await faceServiceClient.AddPersonFaceAsync(personGroupId, person.PersonId, s);
                }
            }
        }

        public async void TrainingAI(string personGroupId)
        {
            await faceServiceClient.TrainPersonGroupAsync(personGroupId);
            TrainingStatus trainingStatus = null;
            while (true)
            {
                trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupId);
                if(trainingStatus.Status != Status.Running)
                {
                    break;
                }
                await Task.Delay(1000);
            }

            Console.WriteLine("Training AI Completed");
        }

        public async void RecognitionFace(string personGroupId, string imagePath)
        {   
            using (Stream s = File.OpenRead(imagePath))
            {
                var faces = await faceServiceClient.DetectAsync(s);
                var faceIds = faces.Select(face => face.FaceId).ToArray();

                try
                {
                    var results = await faceServiceClient.IdentifyAsync(personGroupId, faceIds);
                    foreach(var identifyResult in results)
                    {
                        Console.WriteLine($"Kết quả: {identifyResult.FaceId}");
                        if(identifyResult.Candidates.Length == 0)
                        {
                            Console.WriteLine("Không nhận diện");
                        }
                        else
                        {
                            var candidateId = identifyResult.Candidates[0].PersonId;
                            var person = await faceServiceClient.GetPersonAsync(personGroupId, candidateId);
                            Console.WriteLine($"Xác định là: {person.Name}");
                        }
                    }
                }catch(Exception e)
                {
                    Console.WriteLine("Lỗi nhận dạng khuôn mặt: " + e.Message);
                }
            }
        }
        static void Main(string[] args)
        {
            new Program().CreatePersonGroup("duonghai", "duonghai");
            new Program().AddPersonToGroup("duonghai", "Hai", @"D:\FaceAPI\CMH\");
            new Program().AddPersonToGroup("duonghai", "Duong", @"D:\FaceAPI\HVD\");
            new Program().AddPersonToGroup("newtest", "Quang Hai", @"D:\FaceAPI\QuangHai\");

            new Program().TrainingAI("duonghai");

            new Program().RecognitionFace("duonghai", @"D:\FaceAPI\hvd.jpg");

            Console.ReadLine();
        }
    }
}

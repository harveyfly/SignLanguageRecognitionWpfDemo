using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq;

namespace SignLanguageRecognitionWpfDemo
{
    public class SysParameter : INotifyPropertyChanged
    {
        [JsonProperty]
        public int keyframes_num = 24;
        [JsonProperty]
        public int frame_len = 24;
        [JsonProperty]
        public int crop_size = 256;

        public int frame_diff_queue_len = 120;

        private float m_keyFrameThreshold = 20.0f;

        public float keyFrameThreshold 
        {
            get { return m_keyFrameThreshold; }
            set 
            {
                if (value != m_keyFrameThreshold)
                {
                    m_keyFrameThreshold = value;
                    Notify("keyFrameThreshold");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Notify(string propertyName)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public override string ToString()
        {
            return string.Format("Key Frames Num: {0}, One Frame Length: {1}, Image Crop Size: {2}", this.keyframes_num, this.frame_len, this.crop_size);
        }
    }

    public class PostSkeletonData
    {
        public SysParameter parameter;
        public List<float> LastFrameSkeletonData { get; private set; }
        private List<List<float>> KeyFramesSkeletonDataList;
        private Queue<KeyValuePair<float, bool>> FramesDiffQueue;
        public float MaxFrameDiff { get; private set; }
        private bool StartRecord = false;
        private bool IsRecording = false;
        private bool DataReady = false;
        private int StaticFrameCount = 0;

        public PostSkeletonData(SysParameter parameter)
        {
            this.parameter = parameter;
            this.MaxFrameDiff = 5.0f;
            this.KeyFramesSkeletonDataList = new List<List<float>>();
            this.LastFrameSkeletonData = new List<float>();
            this.FramesDiffQueue = new Queue<KeyValuePair<float, bool>>();
        }

        public List<float> getKeyFramesSkeletonData()
        {
            if (DataReady == false) return null;
            if (this.KeyFramesSkeletonDataList.Count < this.parameter.keyframes_num)
            {
                this.KeyFramesSkeletonDataList.Clear();
                DataReady = false;
                return null;
            }
            List<float> KeyFramesSkeletonData = new List<float>();
            foreach(var dataList in this.KeyFramesSkeletonDataList)
            {
                KeyFramesSkeletonData.AddRange(dataList);
            }
            this.KeyFramesSkeletonDataList.Clear();
            DataReady = false;
            return KeyFramesSkeletonData;
        }

        public List<KeyValuePair<float, bool>> getFrameDiffList()
        {
            List<KeyValuePair<float, bool>> FrameDiffList = new List<KeyValuePair<float, bool>>();
            foreach(var frameDiffData in this.FramesDiffQueue)
            {
                FrameDiffList.Add(frameDiffData);
            }
            return FrameDiffList;
        }

        public bool setLastFrameSkeletonData(List<float> skeletonDataList)
        {
            if(skeletonDataList.Count != this.parameter.frame_len || DataReady == true) return false;
            if(this.LastFrameSkeletonData.Count == skeletonDataList.Count)
            {
                //计算与上一帧的差分
                float diffSum = 0.0f;
                for(int i = 0;i < skeletonDataList.Count; i++)
                {
                    diffSum += Math.Abs(LastFrameSkeletonData[i] - skeletonDataList[i]);
                }

                bool selected = false;
                //差分小于阈值
                if (diffSum <= this.parameter.keyFrameThreshold)
                {
                    StaticFrameCount++;
                    if(StaticFrameCount > 10)
                    {
                        StartRecord = !IsRecording;
                        DataReady = !StartRecord;
                    }
                }
                //差分大于阈值
                else
                {
                    StaticFrameCount = 0;
                    if(StartRecord == true)
                    {
                        IsRecording = true;
                        this.KeyFramesSkeletonDataList.Add(skeletonDataList);
                        selected = true;
                    }
                    else
                    {
                        IsRecording = false;
                    }
                }

                this.MaxFrameDiff = Math.Max(MaxFrameDiff, diffSum);
                this.FramesDiffQueue.Enqueue(new KeyValuePair<float, bool>(diffSum, selected));
                if (this.FramesDiffQueue.Count > this.parameter.frame_diff_queue_len)
                {
                    this.FramesDiffQueue.Dequeue();
                }
            }
            this.LastFrameSkeletonData = skeletonDataList;
            return true;
        }
    }

    public class PostData
    {
        [JsonProperty]
        public SysParameter parameter { get; set; }

        [JsonProperty]
        public List<float> skeleton_data { get; set; }

        public PostData(SysParameter parameter, List<float> skeleton_data)
        {
            this.parameter = parameter;
            this.skeleton_data = skeleton_data;
        }
    }

    public class RecognitionResult : INotifyPropertyChanged
    {
        [JsonProperty]
        public bool sucess { get; set; }

        private string m_prediction = "Unknow";
        [JsonProperty]
        public string prediction 
        {
            get { return m_prediction; }
            set 
            {
                if(value != m_prediction)
                {
                    m_prediction = value;
                    Notify("prediction");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Notify(string propertyName)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public override string ToString()
        {
            return string.Format("Prediction result: {0}", prediction);
        }
    }

    public class JsonPostDataConvert : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(PostData));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject containerObj = new JObject();
            PostData postdata = value as PostData;
            containerObj.Add("keyframes_num", postdata.parameter.keyframes_num);
            containerObj.Add("frame_len", postdata.parameter.frame_len);
            containerObj.Add("skeleton_data", JToken.FromObject(postdata.skeleton_data));
            containerObj.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public class SLRHttpClient
    {
        private HttpClient SLRClient = new HttpClient();

        public SLRHttpClient(string ServerSocket)
        {
            this.SLRClient.BaseAddress = new Uri(string.Format("http://{0}", ServerSocket));
            this.SLRClient.DefaultRequestHeaders.Add("User-Agent", "C# WPF program-SLR Demo");
            this.SLRClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<SysParameter> getSysParameterAsync()
        {
            string url = "getSysParameter";
            HttpResponseMessage response = await this.SLRClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string resp = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<SysParameter>(resp);
        }

        public async Task<RecognitionResult> getRecognitionResult(PostData postdata)
        {
            string url = "predict";

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new JsonPostDataConvert());
            settings.Formatting = Formatting.Indented;
            string json = JsonConvert.SerializeObject(postdata, settings);
            StringContent data = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await this.SLRClient.PostAsync(url, data);
            response.EnsureSuccessStatusCode();
            string resp = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<RecognitionResult>(resp);
        }
    }
}

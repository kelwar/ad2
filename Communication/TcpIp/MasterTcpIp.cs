﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Communication.Annotations;
using Communication.Interfaces;
using Communication.Settings;
using Library.Async;
using Library.Logs;

namespace Communication.TcpIp
{
    public class MasterTcpIp : INotifyPropertyChanged, IDisposable
    {
        #region fields

        private TcpClient _terminalClient;
        private NetworkStream _terminalNetStream;

        private string _statusString;
        private bool _isConnect;
        private bool _isRunDataExchange;

        private readonly string _ipAddress;              //Ip
        private readonly int _ipPort;                    //порт
        private readonly int _timeRespoune;              //время на ответ
        private readonly byte _numberTryingTakeData;     //кол-во попыток ожидания ответа до переподключения
        private byte _countTryingTakeData;               //счетчик попыток

        #endregion




        #region ctor

        public MasterTcpIp(string ipAddress, int ipPort, int timeRespoune, byte numberTryingTakeData)
        {
            _ipAddress = ipAddress;
            _ipPort = ipPort;
            _timeRespoune = timeRespoune;
            _numberTryingTakeData = numberTryingTakeData;
        }

        public MasterTcpIp(XmlMasterSettings settings)
            : this(settings.IpAdress, settings.IpPort, settings.TimeRespoune, settings.NumberTryingTakeData)
        {
        }

        #endregion




        #region prop   

        public string StatusString
        {
            get { return _statusString; }
            set
            {
                if (value == _statusString) return;
                _statusString = value;
                OnPropertyChanged();
            }
        }

        public bool IsConnect
        {
            get { return _isConnect; }
            set
            {
                if (value == _isConnect) return;
                _isConnect = value;
                OnPropertyChanged();
            }
        }

        public bool IsRunDataExchange
        {
            get { return _isRunDataExchange; }
            set
            {
                if (value == _isRunDataExchange) return;
                _isRunDataExchange = value;
                OnPropertyChanged();
            }
        }

        #endregion




        #region Method

        public async Task ReConnect()
        {
            OnPropertyChanged(nameof(IsConnect));
            IsConnect = false;
            _countTryingTakeData = 0;
            Dispose();

            await ConnectTcpIp();
        }


        private async Task ConnectTcpIp()
        {
            while (!IsConnect)
            {
                try
                {
                    _terminalClient = new TcpClient { NoDelay = false };  //true - пакет будет отправлен мгновенно (при NetworkStream.Write). false - пока не собранно значительное кол-во данных отправки не будет.
                    IPAddress ipAddress = IPAddress.Parse(_ipAddress);
                    StatusString = $"Conect to {ipAddress} : {_ipPort} ...";

                    await _terminalClient.ConnectAsync(ipAddress, _ipPort);
                    _terminalNetStream = _terminalClient.GetStream();
                    if (_terminalNetStream.DataAvailable)
                    {
                        //Log.log.Warn($"Условие ответа при коннекте для Синерго сработало, игнорим его");
                        var buf = new byte[1024];
                        var nByteTake = await _terminalNetStream.ReadAsync(buf, 0, 5);
                    }
                    IsConnect = true;
                    return;
                }
                catch (Exception ex)
                {
                    IsConnect = false;
                    StatusString = $"Ошибка инициализации соединения: \"{ex.Message}\"";
                    //LogException.WriteLog("Инициализация: ", ex, LogException.TypeLog.TcpIp);
                    Log.log.Fatal(StatusString);
                    Dispose();
                }
            }
        }



        private async Task ConnectTcpIpWithAuthorization()
        {
            var aesKey = "000102030405060708090A0B0C0D0E0F";
            var aesVector = "0F0E0D0C0B0A09080706050403020100";
            var permuteKey = "0D80791EB24BD2126D70A7C77D215888";
            while (!IsConnect)
            {
                try
                {
                    _terminalClient = new TcpClient { NoDelay = false };  //true - пакет будет отправлен мгновенно (при NetworkStream.Write). false - пока не собранно значительное кол-во данных отправки не будет.
                    IPAddress ipAddress = IPAddress.Parse(_ipAddress);
                    StatusString = $"Conect to {ipAddress} : {_ipPort} ...";

                    await _terminalClient.ConnectAsync(ipAddress, _ipPort);
                    _terminalNetStream = _terminalClient.GetStream();

                    IsConnect = IsAccessAllowed(aesKey, aesVector, permuteKey);

                    return;
                }
                catch (Exception ex)
                {
                    IsConnect = false;
                    StatusString = $"Ошибка инициализации соединения: \"{ex.Message}\"";
                    //LogException.WriteLog("Инициализация: ", ex, LogException.TypeLog.TcpIp);
                    Dispose();
                }
            }
        }


        public async Task<bool> RequestAndRespoune(IExchangeDataProviderBase dataProvider)
        {
            if (!IsConnect)
                return false;

            if (dataProvider == null)
                return false;

            IsRunDataExchange = true;
            if (await SendData(dataProvider))
            {
                try
                {
                    //var data = await TakeData(dataProvider.CountSetDataByte, _timeRespoune, CancellationToken.None);
                    var data = await TakeDataAccurate(dataProvider.CountSetDataByte, _timeRespoune, CancellationToken.None);
                    dataProvider.SetDataByte(data);
                    _countTryingTakeData = 0;
                }
                catch (OperationCanceledException)
                {
                    StatusString = "операция  прерванна";

                    //Log.log.Warn(StatusString);
                    if (++_countTryingTakeData > _numberTryingTakeData)
                        await ReConnect();

                    //Log.log.Warn($"Произошёл реконнект: операция отменена");
                    return false;
                }
                catch (TimeoutException)
                {
                   StatusString = $"Время на ожидание ответа вышло.";
                    //Log.log.Warn(StatusString);
                    if (++_countTryingTakeData > _numberTryingTakeData)
                        await ReConnect();
                    //Log.log.Warn($"Произошёл реконнект из-за ошибки таймаута");
                    return false;
                }
                catch (IOException)
                {
                    await ReConnect();

                    //Log.log.Warn($"Произошёл реконнект: ошибка ввода вывода");
                    return false;
                }
            }
            else                                                           //не смогли отрпавить данные. СРАЗУ ЖЕ переподключение
            {
                await ReConnect();
                return false;
            }
            IsRunDataExchange = false;
            return true;
        }


        public async Task<bool> SendData(IExchangeDataProviderBase dataProvider)
        {
            byte[] buffer = dataProvider.GetDataByte();
            try
            {
                if (_terminalClient != null && _terminalNetStream != null && _terminalClient.Client != null && _terminalClient.Client.Connected)
                {
                    await _terminalNetStream.WriteAsync(buffer, 0, buffer.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                StatusString = $"ИСКЛЮЧЕНИЕ SendDataToServer : {ex.Message}. Устройство: {_ipAddress}:{_ipPort}";
                Log.log.Fatal(StatusString);
            }
            return false;
        }


        public async Task<byte[]> TakeData(int nbytes, int timeOut, CancellationToken ct)
        {
            byte[] bDataTemp = new byte[256];

            //int nByteTake=0;
            //while (true)
            //{
            //    nByteTake = _terminalNetStream.Read(bDataTemp, 0, nbytes);
            //    Task.Delay(500);
            //}

            //TODO: создать task в котором считывать пока не считаем нужное кол-во байт. Прерывать этот task по таймауту  AsyncHelp.WithTimeout
            int nByteTake = await AsyncHelp.WithTimeout(_terminalNetStream.ReadAsync(bDataTemp, 0, nbytes, ct), timeOut, ct);
            /*string log = "";
            for (int i = 0; i < nbytes; i++)
                log += (char)bDataTemp[i];
            Log.log.Info($"Считанный ответ: {log}");*/
            if (nByteTake == nbytes)
            {
                var bData = new byte[nByteTake];
                Array.Copy(bDataTemp, bData, nByteTake);
                /*log = "";
                for (int i = 0; i < nByteTake; i++)
                    log += (char)bData[i];
                Log.log.Info($"Скопированный ответ: {log}");*/
                return bData;
            }
            //Log.log.Fatal($"Ответ не прошёл валидацию. Ожидание: {nbytes}, реальность: {nByteTake}");
            return null;
        }

        /// <summary>
        /// Получение данных с указанием таймаута.
        /// Пока nbytes не полученно за время таймаута данные принимаются 
        /// </summary>
        public async Task<byte[]> TakeDataAccurate(int nbytes, int timeOut, CancellationToken ct)
        {
            byte[] bDataTemp = new byte[1024];
            var taskNByteTake = Task.Run(async () =>
            {
                int nByteTake = 0;
                while (nByteTake < nbytes)
                {
                    nByteTake += await _terminalNetStream.ReadAsync(bDataTemp, nByteTake, nbytes, ct);
                    /*string log = "";
                    for (int i = 0; i < nbytes; i++)
                        log += (char)bDataTemp[i];
                    Log.log.Info($"Считанный ответ: {log}");
                    if (nByteTake > nbytes)
                    {
                        log = "";
                        for (int i = 0; i < nByteTake; i++)
                            log += (char)bDataTemp[i];
                        Log.log.Info($"Ожидание: {nbytes}; реальность {nByteTake}; " + log);
                    }*/
                }
                return nByteTake;
            }, ct);

            int resultNByteTake = await AsyncHelp.WithTimeout(taskNByteTake, timeOut, ct);
            if (resultNByteTake == nbytes)
            {
                var bData = new byte[resultNByteTake];
                Array.Copy(bDataTemp, bData, resultNByteTake);
                /*string log = "";
                for (int i = 0; i < resultNByteTake; i++)
                    log += (char)bData[i];
                Log.log.Info($"Скопированный ответ: {log}");*/
                return bData;
            }
            //Log.log.Fatal($"Ответ не прошёл валидацию. Ожидание: {nbytes}, реальность: {resultNByteTake}");
            return null;
        }

        // Проверка аутентификации на табло
        private bool IsAccessAllowed(string aesKey, string aesVector, string permuteKey)
        {
            var isAuth = false;

            if (_terminalNetStream != null && _terminalNetStream.DataAvailable)
            {
                var tcpBuffer = new byte[1024];

                Thread.Sleep(1000); // ждём ответа от табло

                var bret = _terminalNetStream.Read(tcpBuffer, 0, tcpBuffer.Length);

                //simple access without auth
                if (bret == 5 && !isAuth)
                {
                    isAuth = Encoding.UTF8.GetString(tcpBuffer, 0, bret) == "ready" ? true : false;
                }

                // access with AES algo
                if (bret == 16 && !isAuth)
                {
                    var cryptedBuffer = new byte[16];
                    var decryptedBuffer = new byte[16];
                    int j;
                    for (j = 0; j < 16; j++)
                        cryptedBuffer[j] = tcpBuffer[j];

                    var _aesKey = StringToByteArray(aesKey);
                    var _aesVector = StringToByteArray(aesVector);
                    var _permuteKey = StringToByteArray(permuteKey);

                    RijndaelManaged aes128 = new RijndaelManaged();
                    aes128.Mode = CipherMode.CBC;
                    aes128.Padding = PaddingMode.Zeros;

                    ICryptoTransform d = aes128.CreateDecryptor(_aesKey, _aesVector);

                    decryptedBuffer = d.TransformFinalBlock(cryptedBuffer, 0, 16);

                    for (j = 0; j < 16; j++)
                        decryptedBuffer[j] = (byte)(decryptedBuffer[j] ^ _permuteKey[j]);

                    ICryptoTransform e = aes128.CreateEncryptor(_aesKey, _aesVector);

                    cryptedBuffer = e.TransformFinalBlock(decryptedBuffer, 0, 16);


                    _terminalNetStream.Write(cryptedBuffer, 0, 16);

                    bret = _terminalNetStream.Read(tcpBuffer, 0, tcpBuffer.Length);

                    //simple access without auth
                    if (bret == 5 && !isAuth)
                    {
                        isAuth = Encoding.UTF8.GetString(tcpBuffer, 0, bret) == "ready" ? true : false;
                    }
                }
            }

            return isAuth;
        }

        private byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        #endregion




        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion




        #region Disposable

        public void Dispose()
        {
            if (_terminalNetStream != null)
            {
                _terminalNetStream.Close();
                StatusString = "Сетевой поток закрыт ...";
            }

            _terminalClient?.Client?.Close();
        }

        #endregion
    }
}

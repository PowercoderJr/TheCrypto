# The Crypto
Почтовый агент со сквозным шифрованием **(УЧЕБНЫЙ ПРОЕКТ)**

## Техническое задание
- для отправки почты использовать протокол SMTP;
- для получения почты использовать один из протоколов электронной почты: IMAP или POP3;
- для безопасной аутентификации использовать защищенные протоколы аутентификации;
- отправка и получение почты с вложенными файлами;
- чтение писем на русском языке;
- форматирование письма;
- возможность изменения параметров для подключения к почтовому серверу: название почтового сервера, login, password, порт протокола и т.д.;
- возможность создания нескольких почтовых ящиков на одном клиенте и переключение между ними;
- сохранение писем в ящике (должно быть предусмотрено хранение писем в папке «входящие», «отправленные», «черновики», «корзина» в каждом из настроенных почтовых ящиков);
- выполнение синхронизации папок клиента с папками на почтовом сервере;
- использование криптографических алгоритмов для шифрования почтового сообщенияи использование ЭЦП:
  - шифрование тела сообщения симметричным алгоритмом шифрования;
  - шифрование ключа симметричного алгоритма ассиметричным алгоритмов;
  - получение дайжеста сообщения с помощью функции хеширования;
  - реализация ЭЦП с помощью ассиметричного алгоритма;
- разработка понятного пользовательского интерфейса;
- тестирование почтового клиента в реальных условиях.

## Скриншоты
### Форма авторизации
![Форма авторизации](https://i.imgur.com/k0LnwtG.png)
<br>
<br>
### Главное окно
![Главное окно](https://i.imgur.com/C8nGPFs.png)
<br>
<br>
### Редактирование почтового ящика
![Редактирование почтового ящика](https://i.imgur.com/IQBDQrF.png)
<br>
<br>
### Создание криптографического ключа
![Создание криптографического ключа](https://i.imgur.com/ryzutHq.png)
<br>
<br>
### Менеджер ключей
![Менеджер ключей](https://i.imgur.com/r4xPXxa.png)
<br>
<br>
### Написание письма
![Форма написания письма](https://i.imgur.com/wUZdXPo.png)
<br>
<br>
### Письмо с открытым ключом
![Письмо с открытым ключом](https://i.imgur.com/OVpaN91.png)
<br>
<br>
### Чтение письма - подписано, не зашифровано, есть ключи
![Чтение письма - подписано, не зашифровано, есть ключи](https://i.imgur.com/rXtc2co.png)
<br>
<br>
### Чтение письма - подписано, зашифровано - нет ключей
![Чтение письма - подписано, зашифровано - нет ключей](https://i.imgur.com/hmpnTth.png)
<br>
<br>
### Чтение письма - подписано, зашифровано - есть только ключ ЭЦП
![Чтение письма - подписано, зашифровано - есть только ключ ЭЦП](https://i.imgur.com/qk8ktzm.png)

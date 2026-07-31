# SmartIT Pro v1.0 Mimari Özeti

SmartIT Pro, ilk sürümdeki katmanlı yaklaşımı koruyarak dört ana üretim katmanına ayrılır.

## Domain

`SmartIT.Domain`, Employee, Department, Asset, AssetAssignment, Ticket, SoftwareLicense, MaintenanceSchedule ve AuditLog varlıklarını barındırır. Bu katman başka proje bağımlılığı taşımaz.

## Application

`SmartIT.Application`, MediatR komut/sorgularını, DTO'ları, repository sözleşmelerini ve FluentValidation kurallarını içerir. UI veya EF Core detaylarına doğrudan bağımlı değildir.

## Infrastructure

`SmartIT.Infrastructure`, EF Core SQLite bağlantısını, ASP.NET Core Identity'yi, repository uygulamalarını, dashboard sorgularını ve demo veri başlangıcını sağlar.

## Web

`SmartIT.Web`, asıl SmartIT Pro ürünüdür. MVC controller/view yapısı, cookie authentication, SignalR hub, dosya yükleme, raporlar ve responsive arayüz burada bulunur.

## API

`SmartIT.API`, aynı Application ve Infrastructure katmanlarını kullanan geliştirici entegrasyon yüzeyidir. Web panelinin yerine geçmez; bağımsız istemciler için JWT korumalı endpoint'ler sunar.

## Yerel veri akışı

Web uygulaması ilk açılışta `smartit-v1.db` dosyasını oluşturur. Identity tabloları, uygulama tabloları, roller ve örnek kayıtlar `DbInitializer` tarafından hazırlanır. Böylece SQL Server kurulmadan proje denenebilir.

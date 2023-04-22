Домашнее задание 1 - Развернуть инфраструктуру сервисов

Требуется: 
1. Создать репозиторий проекта Route256Five в https://gitlab.ozon.dev/cs/classroom-5/Week-1
2. Создать проект Ozon.Route256.Five.OrderService
3. Описать файл Dockerfile для Ozon.Route256.Five.OrderService
4. Описать файл docker-compose.yaml
 - kafka
 - postgress (база данных costomers, база данных orders)
 - CustomerService, LogisticsSimulator, OrdersGenerator, ServiceDiscovery - на базе образов из  gitlab-registry.ozon.dev 
 - OrderService - на базе Dockerfile
 - jaeger
5. Создать 2 instance для OrderService
6. Закоммитить проект


Конечный результат: 
1. Присутствуют images: CustomerService, OrdersService, LogisticsSimulator, OrderGenerator, ServiceDiscovery, kafka, postgres, jaeger
2. Корректно стартуют контейнеры: CustomerService, OrderService, LogisticsSimulator, OrdersGenerator, ServiceDiscovery, kafka, postgres, jaeger




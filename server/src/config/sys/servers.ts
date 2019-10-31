export default {
    "development": {
        "login": [
            { "id": "login", "host": "127.0.0.1", "port": 4010 }
        ],
        "connector": [
            { "id": "connector-1", "host": "127.0.0.1", "clientHost": "192.168.1.101", "port": 4011, "frontend": true, "clientPort": 4012 }
        ],
        "info": [
            { "id": "info-1", "host": "127.0.0.1", "port": 4025 }
        ],
        "gameMain": [
            { "id": "gameMain", "host": "127.0.0.1", "port": 4030 }
        ],
        "match": [
            { "id": "match-1", "host": "127.0.0.1", "port": 4040 }
        ],
        "rank": [
            { "id": "rank-1", "host": "127.0.0.1", "port": 4045 }
        ],
        "game": [
            { "id": "game-1", "host": "127.0.0.1", "port": 4050 }
        ]

    },
    "production": {
        "login": [
            { "id": "login", "host": "127.0.0.1", "port": 4010 }
        ],
        "connector": [
            { "id": "connector-1", "host": "127.0.0.1", "clientHost": "129.28.148.167", "port": 4011, "frontend": true, "clientPort": 4012 }
        ],
        "info": [
            { "id": "info-1", "host": "127.0.0.1", "port": 4025 }
        ],
        "gameMain": [
            { "id": "gameMain", "host": "127.0.0.1", "port": 4030 }
        ],
        "match": [
            { "id": "match-1", "host": "127.0.0.1", "port": 4040 }
        ],
        "rank": [
            { "id": "rank-1", "host": "127.0.0.1", "port": 4045 }
        ],
        "game": [
            { "id": "game-1", "host": "127.0.0.1", "port": 4050 }
        ]
    }
}

import redis = require("redis");

export class redisPool {
    private redis_arr: redis.RedisClient[] = [];
    private len: number = 0;

    constructor(config_list: redis.ClientOpts[]) {
        for (let one of config_list) {
            let client = create_redis_client(one);
            this.redis_arr.push(client);
            this.len++;
        }
    }

    /**
     * 获取对应redis client
     * @param id 
     */
    get(id: number) {
        return this.redis_arr[id % this.len];
    }
}

/**
 * 创建redis client
 * @param config 
 */
export function create_redis_client(config: redis.ClientOpts) {
    let client = redis.createClient(config);
    client.on("error", (err) => {
        console.log("redis error", config);
    });
    return client;
}
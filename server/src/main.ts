import express from 'express';
import * as sqlite3 from 'sqlite3';
import { open } from 'sqlite';

interface Data {
    shipId: number,
    baseExp: number
}

(async () => {
    // @ts-ignore
    const db = await open({
        filename: 'base_exp.db',
        driver: sqlite3.Database
    });

    // Create table if not exists
    await db.exec(`
        CREATE TABLE IF NOT EXISTS base_exp (
            uniqueId INTEGER PRIMARY KEY AUTOINCREMENT,
            shipId INTEGER,
            baseExp INTEGER
        )
    `);

    const app = express();
    app.use(express.json());
    const PORT = 4000;
    // @ts-ignore
    app.post("/insert", async (req, res) => {
        const data: Data = req.body;

        await db.run(`
            INSERT INTO base_exp (shipId, baseExp)
            VALUES (?, ?)
        `, [data.shipId, data.baseExp]);

        res.sendStatus(200);
    });

    app.listen(PORT, () => console.log(`Running on port ${PORT}`));
})();
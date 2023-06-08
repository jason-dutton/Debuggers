import * as tedious from 'tedious';
import { Request, Response } from 'express';
import IUser from '../../models/user.interface';
import { connection as conn } from '../../main';
export default class UserController {
  public getAllUsers = (req: Request, res: Response) => {
    try {
      const query = 'SELECT * FROM [dbo].[users]';
      const users: IUser[] = [];

      const request = new tedious.Request(
        query,
        (err: tedious.RequestError, rowCount: number) => {
          if (err) {
            console.log(err);
          } else {
            console.log(rowCount);
          }
        }
      );

      request.on('row', (columns: tedious.ColumnValue[]) => {
        const user: IUser = {
          userId: columns[0].value,
          email: columns[1].value,
          password: columns[2].value,
          userRole: columns[3].value,
        };

        users.push(user);
      });

      request.on('requestCompleted', () => {
        res.status(200);
        res.json({ users: users });
      });
      console.log(
        '🚀 ~ file: user.controller.ts ~ line 55 ~ UserController ~ getAllUsers ~ request'
      );
      conn.execSql(request);
    } catch (error) {
      res.status(500).send(error);
    }
  };
  public getUser = (req: Request, res: Response) => {
    const { userId } = req.params;
    const query = `SELECT * FROM [User] WHERE userId = ${userId}`;

    conn.on('connect', (err: tedious.ConnectionError) => {
      if (err) {
        console.log(err);
      } else {
        const request = new tedious.Request(
          query,
          (err: tedious.RequestError, rowCount: number) => {
            if (err) {
              console.log(err);
            } else {
              console.log(rowCount);
            }
          }
        );

        request.on('row', (columns: tedious.ColumnValue[]) => {
          const user: IUser = {
            userId: columns[0].value,
            email: columns[1].value,
            password: columns[2].value,
            userRole: columns[3].value,
          };

          res.send(user);
        });

        conn.execSql(request);
      }
    });
  };

  public createUser = (req: Request, res: Response) => {
    const { email, password, userRole } = req.body;
    const query = `INSERT INTO [User] (email, password, userRole) VALUES ('${email}', '${password}', '${userRole}')`;

    conn.on('connect', (err: tedious.ConnectionError) => {
      if (err) {
        console.log(err);
      } else {
        const request = new tedious.Request(
          query,
          (err: tedious.RequestError, rowCount: number) => {
            if (err) {
              console.log(err);
            } else {
              console.log(rowCount);
            }
          }
        );

        conn.execSql(request);
      }
    });
  };
  public updateUser = (req: Request, res: Response) => {
    const { userId } = req.params;
    const { email, password, userRole } = req.body;
    const query = `UPDATE [User] SET email = '${email}', password = '${password}', userRole = '${userRole}' WHERE userId = ${userId}`;

    conn.on('connect', (err: tedious.ConnectionError) => {
      if (err) {
        console.log(err);
      } else {
        const request = new tedious.Request(
          query,
          (err: tedious.RequestError, rowCount: number) => {
            if (err) {
              console.log(err);
            } else {
              console.log(rowCount);
            }
          }
        );

        conn.execSql(request);
      }
    });
  };
}

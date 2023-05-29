import * as tedious from 'tedious';
import { Request, Response } from 'express';
import IUser from '../../models/user.interface';

export default class UserController {
  constructor(private connection: tedious.Connection) {}

  public getAllUsers = (req: Request, res: Response) => {
    const query = 'SELECT * FROM [User]';
    const users: IUser[] = [];

    this.connection.on('connect', (err: tedious.ConnectionError) => {
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

          users.push(user);
        });

        request.on('requestCompleted', () => {
          res.send(users);
        });

        this.connection.execSql(request);
      }
    });
  };
}
// public getUser = (req: Request, res: Response) => {
//     const { userId } = req.params;
//     const query = `SELECT * FROM [User] WHERE userId = ${userId}`;

//     this.connection.on('connect', (err: tedious.ConnectionError) => {
//       if (err) {
//         console.log(err);
//       } else {
//         const request = new tedious.Request(
//           query,
//           (err: tedious.RequestError, rowCount: number) => {
//             if (err) {
//               console.log(err);
//             } else {
//               console.log(rowCount);
//             }
//           }
//         );

//         request.on('row', (columns: tedious.ColumnValue[]) => {
//           const user: IUser = {
//             userId: columns[0].value,
//             email: columns[1].value,
//             password: columns[2].value,
//             userRole: columns[3].value,
//           };

//           res.send(user);
//         });

//         this.connection.execSql(request);
//       }
//     });
//   };

import { connection as conn } from '../../main';
import { Request, Response } from 'express';
import * as tedious from 'tedious';

export default class AuthController {
  public registerUser = (req: Request, res: Response) => {
    try {
      const { email, password, userRole } = req.body;

      //Create datacreated variable with timestamp
      const dateCreated = new Date()
        .toISOString()
        .slice(0, 19)
        .replace('T', ' ');
      const query = `INSERT INTO [dbo].[users] (email, password, userRole, dateCreated) VALUES ('${email}', '${password}', '${userRole}', '${dateCreated}')`;

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

      res.status(200).json({
        message: 'User registered successfully.',
      });
    } catch (error) {
      res.status(500).json({
        error: error,
        details: 'Email already exists.',
      });
    }
  };
}

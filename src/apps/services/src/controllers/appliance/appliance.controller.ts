import * as tedious from 'tedious';
import { Request, Response } from 'express';
import IAppliance from '../../models/appliance.interface';
import { connection as conn } from '../../main';
export default class ApplianceController {
  public createAppliance = (req: Request, res: Response) => {
    const { type, powerUsage, durationUsed } = req.body;
    const query =
      `INSERT INTO [dbo].[appliances] (type, powerUsage, durationUsed)` +
      ` VALUES ('${type}', ${powerUsage}, ${durationUsed})`;

    try {
      const request = new tedious.Request(
        query,
        (err: tedious.RequestError, rowCount: number) => {
          if (err) {
            console.log('Express: ' + err.message);
            return res.status(400).json({
              error: err.message,
            });
          } else {
            console.log('Express: Appliance created successfully.');
            return res.status(200).json({
              message: 'Appliance created successfully.',
            });
          }
        }
      );

      conn.execSql(request);
    } catch (error) {
      console.log('Express: ' + error.message);
      res.status(500).json({
        error: error.message,
      });
    }
  };

  public getAllAppliances = (req: Request, res: Response) => {
    const query = 'SELECT * FROM [dbo].[appliances]';
    const appliances: IAppliance[] = [];

    try {
      const request = new tedious.Request(
        query,
        (err: tedious.RequestError, rowCount: number) => {
          if (err) {
            console.log('Express: ' + err.message);
            return res.status(400).json({
              error: err.message,
            });
          } else if (rowCount === 0) {
            console.log('Express: No appliances exist.');
            return res.status(404).json({
              error: 'Not Found',
              details: 'No appliances exist.',
            });
          } else {
            console.log('Express: appliances retrieved successfully.');
            res.status(200).json(appliances);
          }
        }
      );

      request.on('row', (columns: tedious.ColumnValue[]) => {
        const appliance: IAppliance = {
          applianceId: columns[0].value,
          type: columns[1].value,
          powerUsage: columns[2].value,
          durationUsed: columns[3].value,
        };
        appliances.push(appliance);
      });

      conn.execSql(request);
    } catch (error) {
      console.log('Express: ' + error.message);
      res.status(500).json({
        error: error.message,
      });
    }
  };

  public getAppliance = (req: Request, res: Response) => {
    const { applianceId } = req.params;
    let appliance: IAppliance;
    const query = `SELECT * FROM [dbo].[appliances] WHERE applianceId = ${applianceId}`;

    try {
      const request = new tedious.Request(
        query,
        (err: tedious.RequestError, rowCount: number) => {
          if (err) {
            console.log('Express: ' + err.message);
            return res.status(400).json({
              error: err.message,
            });
          } else if (rowCount === 0) {
            console.log('Express: Appliance does not exist.');
            return res.status(404).json({
              error: 'Not Found',
              details: 'Appliance does not exist.',
            });
          } else {
            console.log('Express: Appliance retrieved successfully.');
            res.status(200).json(appliance);
          }
        }
      );

      request.on('row', (columns: tedious.ColumnValue[]) => {
        appliance = {
          applianceId: columns[0].value,
          type: columns[1].value,
          powerUsage: columns[2].value,
          durationUsed: columns[3].value,
        };
      });

      conn.execSql(request);
    } catch (error) {
      console.log('Express: ' + error.message);
      res.status(500).json({
        error: error.message,
      });
    }
  };

  public updateAppliance = (req: Request, res: Response) => {
    const { applianceId } = req.params;
    const { type, powerUsage, durationUsed } = req.body;
    const query =
      `UPDATE [dbo].[appliances] SET type = '${type}', powerUsage = ${powerUsage}, durationUsed = ${durationUsed}` +
      `WHERE applianceId = ${applianceId}`;

    try {
      const request = new tedious.Request(
        query,
        (err: tedious.RequestError, rowCount: number) => {
          if (err) {
            console.log('Express: ' + err.message);
            return res.status(400).json({
              error: err.message,
            });
          } else if (rowCount === 0) {
            console.log('Express: Appliance does not exist.');
            return res.status(404).json({
              error: 'Not Found',
              details: 'Appliance does not exist.',
            });
          } else {
            console.log('Express: Appliance updated successfully.');
            res.status(200).json({
              message: 'Appliance updated successfully.',
            });
          }
        }
      );

      conn.execSql(request);
    } catch (error) {
      res.status(500).json({
        error: error.message,
      });
    }
  };

  public deleteAppliance = async (req: Request, res: Response) => {
    const { applianceId } = req.params;
    const query = `DELETE FROM [dbo].[appliances] WHERE applianceId = ${applianceId}`;

    try {
      const request = new tedious.Request(
        query,
        (err: tedious.RequestError, rowCount: number) => {
          if (err) {
            console.log('Express: ' + err.message);
            return res.status(400).json({
              error: err.message,
            });
          } else if (rowCount === 0) {
            console.log('Express: Appliance does not exist.');
            return res.status(404).json({
              error: 'Not Found',
              details: 'Appliance does not exist.',
            });
          } else {
            console.log('Express: Appliance deleted successfully.');
            res.status(200).json({
              message: 'Appliance deleted successfully.',
            });
          }
        }
      );

      conn.execSql(request);
    } catch (error) {
      console.log('Express: ' + error.message);
      res.status(500).json({
        error: error.message,
      });
    }
  };
}

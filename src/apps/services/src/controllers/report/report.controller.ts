import * as tedious from 'tedious';
import { Request, Response } from 'express';
import IReport from '../../models/report.interface';
import { connection as conn } from '../../main';
export default class ReportController {
  public createReport = (req: Request, res: Response) => {
    const {
      reportName,
      userId,
      basicCalculationId,
      solarScore,
      runningTime,
    } = req.body;

    const dateCreated = new Date()
      .toISOString()
      .slice(0, 19)
      .replace('T', ' ');
      
    const query =
      `INSERT INTO [dbo].[reports] (reportName, userId, basicCalculationId, solarScore, runningTime, dateCreated)` +
      ` VALUES ('${reportName}', ${userId}, ${basicCalculationId}, ${solarScore}, ${runningTime}, '${dateCreated}')`;


    try {

      const request = new tedious.Request(
        query,
        (err: tedious.RequestError, rowCount: number) => {
          if (err) {
            return res.status(400).json({
              error: err.message,
            });
          } else {
            console.log(rowCount);
            return res.status(200).json({
              message: 'Report created successfully.',
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

  public getAllReports = (req: Request, res: Response) => {
    
      const query = 'SELECT * FROM [dbo].[reports]';
      const reports: IReport[] = [];

    try {
      const request = new tedious.Request(
        query,
        (err: tedious.RequestError, rowCount: number) => {
          if (err) {
            return res.status(400).json({
              error: err.message,
            });
          } else if (rowCount === 0) {
            return res.status(404).json({
              error: 'Not Found',
              details: 'No reports exist.',
            });
          } else {
            console.log(rowCount);
            res.status(200).json(reports);
          }
        }
      );

      request.on('row', (columns: tedious.ColumnValue[]) => {
        const report: IReport = {
          reportId: columns[0].value,
          reportName: columns[1].value,
          userId: columns[2].value,
          basicCalculationId: columns[3].value,
          solarScore: columns[4].value,
          runningTime: columns[5].value,
          dateCreated: columns[6].value,
        };

        reports.push(report);
      });

      conn.execSql(request);
    } catch (error) {
      res.status(500).json({
        error: error.message,
      });
    }
  };

  public getReport = (req: Request, res: Response) => {
    const { reportId } = req.params;
    let report: IReport;
    const query = `SELECT * FROM [dbo].[reports] WHERE reportId = ${reportId}`;

    try {
      const request = new tedious.Request(
        query,
        (err: tedious.RequestError, rowCount: number) => {
          if (err) {
            return res.status(400).json({
              error: err.message,
            });
          } else if (rowCount === 0) {
            return res.status(404).json({
              error: 'Not Found',
              details: 'Report does not exist.',
            });
          } else {
            console.log(rowCount);
            res.status(200).json(report);
          }
        }
      );

      request.on('row', (columns: tedious.ColumnValue[]) => {
        report = {
          reportId: columns[0].value,
          reportName: columns[1].value,
          userId: columns[2].value,
          basicCalculationId: columns[3].value,
          solarScore: columns[4].value,
          runningTime: columns[5].value,
          dateCreated: columns[6].value,
        };
      });

      conn.execSql(request);
    } catch (error) {
      res.status(500).json({
        error: error.message,
      });
    }
  };


  public updateReport = (req: Request, res: Response) => {
    const { reportId } = req.params;

    if (!Number.isInteger(Number(reportId))) {
      return res.status(400).json({
        error: 'Invalid reportId',
        details: 'ReportId must be an integer.',
      });
    }

    const { reportName, userId, basicCalculationId, solarScore, runningTime } =
      req.body;
    try {
      const query =
        `UPDATE [dbo].[reports] SET reportName = '${reportName}', userId = ${userId}, basicCalculationId = ${basicCalculationId}, solarScore = ${solarScore}, runningTime = ${runningTime}` +
        `WHERE reportId = ${reportId}`;
      const request = new tedious.Request(
        query,
        (err: tedious.RequestError, rowCount: number) => {
          if (err) {
            return res.status(400).json({
              error: err.message,
            });
          } else if (rowCount === 0) {
            return res.status(401).json({
              error: 'Unauthorized',
              details: 'Report does not exist.',
            });
          } else {
            console.log(rowCount);
          }
        }
      );
      conn.execSql(request);
      request.on('requestCompleted', () => {
        res.status(200).json({
          message: 'Report updated successfully.',
        });
      });
    } catch (error) {
      res.status(500).json({
        error: 'Failed to update report.',
        details: 'Database connection error.',
      });
    }
  };

  public deleteReport = async (req: Request, res: Response) => {
    const { reportId } = req.params;

    if (!Number.isInteger(Number(reportId))) {
      return res.status(400).json({
        error: 'Invalid reportId',
        details: 'reportId must be an integer.',
      });
    }

    try {
      const query = `DELETE FROM [dbo].[reports] WHERE reportId = ${reportId}`;
      const request = new tedious.Request(
        query,
        (err: tedious.RequestError, rowCount: number) => {
          if (err) {
            return res.status(400).json({
              error: err.message,
            });
          } else if (rowCount === 0) {
            return res.status(401).json({
              error: 'Unauthorized',
              details: 'Report does not exist.',
            });
          } else {
            console.log(rowCount);
          }
        }
      );

      request.on('requestCompleted', () => {
        res.status(200).json({
          message: 'Appliance deleted successfully.',
        });
      });
      conn.execSql(request);
    } catch (error) {
      res.status(500).json({
        error: 'Failed to find appliance to delete.',
        details: 'Database connection error.',
      });
    }
  };
}

/*******************************************************************/
/*                                                                 */
/*  allan500 :  stores frequency vs time in a single data file     */
/*                                                                 */
/*  version: see define                                            */
/*  date:    15. July 1999                                         */
/*                                                                 */
/*                                                                 */
/*******************************************************************/

#include <windows.h> // WSC uses LPSTR!
#include <math.h>    
#include <float.h>
#include <stdio.h>
#include <stdlib.h>
#include <time.h>
#include <conio.h>
#include "wsc.h"     // for serial communication


#define		prog_VERSION		"ver 0.90 by Michael Matus, BEV"
#define		ID_counter	03		// IEEE address of counter
#define		MAX_DATA	1000000	// 3 months should be enough

int      port      =  0;        // COM port where 0=COM1 and 1=COM2
double   gate_time =  1.0;      // the gate time in s

/*****   prototypes   *****/
void   init_500(void);
void   HP53131_meas_mode(double);
void   HP53131_disp_mode(void);
double HP53131_get_freq(double);
int    send_500(char *);
int    read_500(char *);
void   sleep(clock_t);

/*********************************************************/

void main(int argc, char *argv[])
{                       
	
long int	i;
double	    f;

FILE *		OutFile;
char		filename[_MAX_PATH], base_filename[_MAX_PATH];
char		laser1[255], laser2[255];
struct		tm *newtime;
time_t		long_time, start_time;


if(argc>2)
{
	printf("! too many parameters, usage: allan500 [filename]\n");
	exit(1);
}
if(argc==1)
{
	printf("filename (without extension): ");
	gets(base_filename);
}
if(argc==2) strcpy(base_filename, argv[1]);


printf("\nThis is allan500, version %s\n\n", prog_VERSION);

strcpy(filename, base_filename);
strcat(filename,".prn");

printf("designation and component of laser #1: ");
gets(laser1);
printf("designation and component of laser #2: ");
gets(laser2);


printf("\ninitializing");
init_500();  // initialisation of COM port and 500-serial
printf(" - done\n");

HP53131_meas_mode(gate_time);

OutFile = fopen(filename,"wt");
time( &long_time );                     /* Get time as long integer. */
newtime = localtime( &long_time );      /* Convert to local time. */

fprintf(OutFile,"output of allan500.exe, version %s\n", prog_VERSION);
fprintf(OutFile,"laser 1: %s\n", laser1);
fprintf(OutFile,"laser 2: %s\n", laser2);
fprintf(OutFile,"filename: %s\n", filename);
fprintf(OutFile,"measurement started at %.19s\n",asctime(newtime));
fprintf(OutFile,"gate time: %3.1Lf s\n", gate_time);
fprintf(OutFile,"time units: s\n");
fprintf(OutFile,"frequency units: MHz\n");
fprintf(OutFile,"@@@@\n");
fflush(OutFile);

time( &start_time);   
for(i=0; i<MAX_DATA; i++)
	{
	time( &long_time );
    f = HP53131_get_freq(gate_time);
	fprintf(OutFile,"%7Li %10.6Lf\n", long_time-start_time, f);
	}

fclose(OutFile);   
HP53131_disp_mode();
SioDone(port);

}

/***************************************************************************/

void init_500(void)
{
	char buffer [1024];

	SioReset(port, 1024, 1024);
	SioParms(port, NoParity, OneStopBit, WordLength8);
	SioBaud(port, Baud9600);
	SioFlow(port, 'H');

	send_500("\r");
	sleep(100);
	send_500("\r");
	sleep(100);
	send_500("\r");
	sleep(100);
	send_500("\r");
	sleep(100);
	send_500("\r");
	sleep(100);
	
	send_500("I\r");
	send_500("EC;0\r");
	send_500("H;1\r");
	send_500("X;0\r");
	send_500("TC;2\r");
	send_500("TB;4\r");

	read_500(buffer);

	send_500("C\r");

}

/***************************************************************************/

int send_500(char *buffer)
{
	return(SioPuts(port, buffer, strlen(buffer)));
}

/***************************************************************************/

int read_500(char *buffer)
{
	return(SioGets(port, buffer, 1024));
}

/***************************************************************************/

void HP53131_meas_mode(double gate_time)
{
	char	buffer[1024];

	sprintf(buffer, "OA;03;:SENS:TOT:ARM:STOP:TIM %5.3Lf\r", gate_time);

	send_500("OA;03;*RST\r");
	send_500("OA;03;*CLS\r");
	send_500("OA;03;*SRE 0\r");
	send_500("OA;03;*ESE 0\r");
	send_500("OA;03;:STAT:PRES\r");
	send_500("OA;03;:FUNC 'TOT 1'\r");
	send_500("OA;03;:INP:COUP AC\r");
	send_500("OA;03;:INP:IMP 50\r");
	send_500("OA;03;:SENS:TOT:ARM:STAR:SOUR IMM\r");
	send_500("OA;03;:SENS:TOT:ARM:STOP:SOUR TIM\r");
	send_500(buffer);  // this sets the gate time
	send_500("OA;03;:SENS:EVEN:LEV:ABS:AUTO OFF\r");
	send_500("OA;03;:DIAG:CAL:INT:AUTO OFF\r");
	send_500("OA;03;:DISP:ENABLE ON\r");
	send_500("OA;03;:HCOPY:CONT OFF\r");
	send_500("OA;03;:CALC:MATH:STATE OFF\r");                  
	send_500("OA;03;:CALC2:LIM:STATE OFF\r");
	send_500("OA;03;:CALC3:AVER:STATE OFF\r");
	send_500("OA;03;:INIT:CONT ON\r");
	sleep(500);
	return;
}

/***************************************************************************/

void HP53131_disp_mode(void)
{
	send_500("OA;03;*RST\r");
	send_500("OA;03;*CLS\r");
	send_500("OA;03;*SRE 0\r");
	send_500("OA;03;*ESE 0\r");
	send_500("OA;03;:STAT:PRES\r");
	send_500("OA;03;:FUNC 'TOT 1'\r");
	send_500("OA;03;:INP:COUP AC\r");
	send_500("OA;03;:INP:IMP 50\r");
	send_500("OA;03;:SENS:TOT:ARM:STAR:SOUR IMM\r");
	send_500("OA;03;:SENS:TOT:ARM:STOP:SOUR TIM\r");
	send_500("OA;03;:SENS:TOT:ARM:STOP:TIM 1.0\r");
	send_500("OA;03;:DISP:ENABLE ON\r");
	send_500("OA;03;:INIT:CONT ON\r");
	sleep(500);
	return;
}

/***************************************************************************/
		
double HP53131_get_freq(double gate_time)
{
	double	f;
	char	r[1024];

	send_500("OA;03;FETCH?\r");
	send_500("EN;03\r");
	sleep((int)(gate_time*1000+50));
	read_500(r);
	sscanf(r, "%Lf", &f);
	f = f / (1000000.0L * gate_time);	// convert to MHz
	return(f);
}

/***************************************************************************/

/* Pauses for a specified number of milliseconds. */

void sleep( clock_t wait )
{
    clock_t goal;

    goal = wait + clock();
    while( goal > clock() );
}


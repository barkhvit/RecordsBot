﻿using RecordBot.CallBackModels;
using RecordBot.Enums;
using RecordBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace RecordBot.Commands
{
    public class CommandsForProcedures : Commands
    {
        public CommandsForProcedures(ITelegramBotClient telegramBotClient, IAppointmentService appointmentService, IProcedureService procedureService) : 
            base(telegramBotClient, appointmentService, procedureService)
        {
        }

        //администрирование процедур, нажатие на /admin -> Услуги
        public async Task AdminProcedureCommand(Update update, CancellationToken cancellationToken)
        {
            await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id);
            await _telegramBotClient.EditMessageText(
                messageId: update.CallbackQuery.Message.MessageId,
                chatId: update.CallbackQuery.Message.Chat.Id,
                text: "Выберите действия с процедурами:",
                cancellationToken: cancellationToken,
                replyMarkup: Keyboards.KeyboardsForProcedures.GetProcedureAdminKeybord()
                );
        }

        //показать все процедуры 
        public async Task ShowActiveProceduresCommand(Update update, CancellationToken cancellationToken, ReasonShowProcedure reasonShowProcedure)
        {
            var procedures = await _procedureService.GetProceduresByActive(true, cancellationToken);
            await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id);
            await _telegramBotClient.EditMessageText(
                messageId: update.CallbackQuery.Message.MessageId,
                chatId: update.CallbackQuery.Message.Chat.Id,
                text: "Список активных процедур:",
                cancellationToken: cancellationToken,
                replyMarkup: Keyboards.KeyboardsForProcedures.GetAllProcedures(procedures, reasonShowProcedure));
        }

        //показать информацию о выбранной процедуре (клавиатура подбирается в зависимости от причины)
        public async Task ShowProcedureCommand(ProcedureCallBackDto procedureCallBackDto, Update update, CancellationToken cancellationToken, ReasonShowProcedure reasonShowProcedure)
        {

            if (procedureCallBackDto.ProcedureId!=null)
            {
                var procedure = await _procedureService.GetProcedureByGuidId((Guid)procedureCallBackDto.ProcedureId, cancellationToken);
                if (procedure != null)
                {
                    //если это резервирование то GetKeybordForReserved, если администрирование то GetKeybordForProcedure
                    InlineKeyboardMarkup? inlineKeyboardMarkup = reasonShowProcedure == ReasonShowProcedure.admin ?
                        Keyboards.KeyboardsForProcedures.GetKeybordForProcedure(procedure) : Keyboards.KeyboardsForProcedures.GetKeybordForReserved(procedure);

                    string status = procedure.isActive == true ? "активна" : "не активна";
                    string messageText = $"Название: {procedure.Name}\nОписание: {procedure.Description}\n" +
                                    $"Стоимость: {procedure.Price} рублей\nДлительность: {procedure.DurationMinutes} минут." +
                                    $"\nСтатус: {status}";
                    await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id);
                    await _telegramBotClient.EditMessageText(
                        chatId: update.CallbackQuery.Message.Chat.Id,
                        messageId: update.CallbackQuery.Message.MessageId,
                        text: messageText,
                        cancellationToken: cancellationToken,
                        replyMarkup: inlineKeyboardMarkup
                        );
                }
            }
        }

        //показать архивные процедуры
        public async Task ShowArchiveProceduresCommand(Update update, CancellationToken cancellationToken, ReasonShowProcedure reasonShowProcedure)
        {
            var procedures = await _procedureService.GetProceduresByActive(false, cancellationToken);
            await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: cancellationToken);
            await _telegramBotClient.SendMessage(
                chatId: update.CallbackQuery.Message.Chat.Id,
                text: "Список процедур в архиве:",
                cancellationToken: cancellationToken,
                replyMarkup: Keyboards.KeyboardsForProcedures.GetAllProcedures(procedures, reasonShowProcedure));
        }

        //меняет активность процедуры на противоположный
        public async Task ChangeIsActiveProcedureCommand(Update update, ProcedureCallBackDto procedureCallBackDto, CancellationToken cancellationToken)
        {
            if (procedureCallBackDto.ProcedureId!=null)
            {
                string messageText = "";
                await _procedureService.ChangeActive((Guid)procedureCallBackDto.ProcedureId, cancellationToken);
                var procedure = await _procedureService.GetProcedureByGuidId((Guid)procedureCallBackDto.ProcedureId, cancellationToken);
                if (procedure != null)
                {
                    messageText = procedure.isActive == true ? "Процедура активна" : "Процедура помещена в архив";
                    await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id);
                    await _telegramBotClient.EditMessageText(
                        chatId: update.CallbackQuery.Message.Chat.Id,
                        messageId: update.CallbackQuery.Message.MessageId,
                        text: messageText,
                        cancellationToken: cancellationToken);
                }
            }
        }

    }
}
